import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CountUp } from '../../shared/directives/count-up';
import { forkJoin, map, Observable, of, switchMap } from 'rxjs';
import { Activities } from '../../core/activities/activities';
import { Activity } from '../../core/activities/activity.models';
import { Auth } from '../../core/auth/auth';
import { Resource } from '../../core/resources/resource.models';
import { Resources } from '../../core/resources/resources';
import { ScheduleDetail, ScheduleSummary } from '../../core/schedules/schedule.models';
import { Schedules } from '../../core/schedules/schedules';

interface KindCount {
  kind: string;
  count: number;
  ratio: number;
}

interface HeatCell {
  day: number;
  hour: number;
  count: number;
  ratio: number;
}

interface Occupancy {
  id: string;
  name: string;
  kind: string;
  bookedMinutes: number;
  capacityMinutes: number;
  ratio: number;
}

interface PriorityCount {
  priority: number;
  count: number;
  ratio: number;
}

const HEAT_HOURS = Array.from({ length: 13 }, (_, i) => 7 + i); // 7h → 19h
const DAY_LABELS = ['Lun', 'Mar', 'Mer', 'Jeu', 'Ven', 'Sam', 'Dim'];
const DEFAULT_CAPACITY_MINUTES = 5 * 10 * 60; // 5 jours × 10 h si aucune disponibilité déclarée

@Component({
  imports: [RouterLink, CountUp],
  selector: 'app-dashboard',
  styleUrl: './dashboard.scss',
  templateUrl: './dashboard.html',
})
export class Dashboard {
  protected readonly auth = inject(Auth);
  private readonly resourcesApi = inject(Resources);
  private readonly activitiesApi = inject(Activities);
  private readonly schedulesApi = inject(Schedules);

  readonly resources = signal<Resource[]>([]);
  readonly activities = signal<Activity[]>([]);
  readonly schedules = signal<ScheduleSummary[]>([]);
  readonly latestDetail = signal<ScheduleDetail | null>(null);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  readonly isAdmin = computed(() => this.auth.user()?.role === 'Admin');
  readonly greeting = computed(() => {
    const h = new Date().getHours();
    return h < 6 ? 'Bonne nuit' : h < 18 ? 'Bonjour' : 'Bonsoir';
  });

  readonly activeResources = computed(
    () => this.resources().filter((r) => r.status === 'Active').length,
  );
  readonly activeActivities = computed(
    () => this.activities().filter((a) => a.status === 'Active').length,
  );
  readonly totalMinutes = computed(() =>
    this.activities()
      .filter((a) => a.status === 'Active')
      .reduce((sum, a) => sum + a.durationMinutes, 0),
  );
  readonly totalHoursLabel = computed(() => {
    const m = this.totalMinutes();
    const h = Math.floor(m / 60);
    return m % 60 === 0 ? `${h} h` : `${h} h ${(m % 60).toString().padStart(2, '0')}`;
  });

  readonly latestSchedule = computed(() => this.schedules()[0] ?? null);
  readonly recentSchedules = computed(() => this.schedules().slice(0, 5));

  readonly resourcesByKind = computed<KindCount[]>(() => {
    const counts = new Map<string, number>();
    for (const r of this.resources()) counts.set(r.kind, (counts.get(r.kind) ?? 0) + 1);
    const max = Math.max(1, ...counts.values());
    return [...counts.entries()]
      .map(([kind, count]) => ({ kind, count, ratio: count / max }))
      .sort((a, b) => b.count - a.count);
  });

  // ---------- visualisations ----------
  readonly heatHours = HEAT_HOURS;
  readonly dayLabels = DAY_LABELS;

  readonly heatmap = computed<HeatCell[][]>(() => {
    const detail = this.latestDetail();
    const grid = DAY_LABELS.map((_, day) =>
      HEAT_HOURS.map((hour) => ({ day, hour, count: 0, ratio: 0 })),
    );
    if (!detail) return grid;

    for (const a of detail.assignments) {
      const start = new Date(a.start);
      const end = new Date(a.end);
      const day = (start.getDay() + 6) % 7;
      const from = start.getHours() + start.getMinutes() / 60;
      const to = end.getHours() + end.getMinutes() / 60;
      for (const cell of grid[day]) {
        if (cell.hour < to && cell.hour + 1 > from) cell.count++;
      }
    }
    const max = Math.max(1, ...grid.flat().map((c) => c.count));
    for (const cell of grid.flat()) cell.ratio = cell.count / max;
    return grid;
  });

  readonly heatMax = computed(() =>
    Math.max(
      0,
      ...this.heatmap()
        .flat()
        .map((c) => c.count),
    ),
  );

  readonly occupancy = computed<Occupancy[]>(() => {
    const detail = this.latestDetail();
    if (!detail) return [];
    const booked = new Map<string, number>();
    for (const a of detail.assignments) {
      const minutes = (new Date(a.end).getTime() - new Date(a.start).getTime()) / 60_000;
      for (const r of a.resources) booked.set(r.id, (booked.get(r.id) ?? 0) + minutes);
    }
    return this.resources()
      .filter((r) => r.status === 'Active')
      .map((r) => {
        const capacityMinutes =
          r.availability.length > 0
            ? r.availability.reduce((sum, w) => sum + (toMinutes(w.end) - toMinutes(w.start)), 0)
            : DEFAULT_CAPACITY_MINUTES;
        const bookedMinutes = booked.get(r.id) ?? 0;
        return {
          id: r.id,
          name: r.name,
          kind: r.kind,
          bookedMinutes,
          capacityMinutes,
          ratio: Math.min(1, bookedMinutes / capacityMinutes),
        };
      })
      .sort((a, b) => b.ratio - a.ratio)
      .slice(0, 8);
  });

  readonly byPriority = computed<PriorityCount[]>(() => {
    const counts = [0, 0, 0, 0, 0];
    for (const a of this.activities()) if (a.status === 'Active') counts[a.priority - 1]++;
    const max = Math.max(1, ...counts);
    return counts.map((count, i) => ({ priority: i + 1, count, ratio: count / max })).reverse();
  });

  readonly placedRatio = computed(() => {
    const detail = this.latestDetail();
    const total = this.activeActivities();
    if (!detail || total === 0) return null;
    const placed = new Set(detail.assignments.map((a) => a.activityId)).size;
    return { placed, total, ratio: placed / total };
  });

  readonly isEmpty = computed(
    () =>
      this.resources().length === 0 &&
      this.activities().length === 0 &&
      this.schedules().length === 0,
  );

  readonly steps = computed(() => [
    {
      label: 'Déclarer des ressources',
      done: this.resources().length > 0,
      link: '/resources',
      hint: 'salles, personnes, équipements',
    },
    {
      label: 'Créer des activités',
      done: this.activities().length > 0,
      link: '/activities',
      hint: 'avec leurs besoins en ressources',
    },
    {
      label: 'Générer un planning',
      done: this.schedules().some((s) => s.assignmentCount > 0),
      link: '/schedules',
      hint: 'à la main ou avec le solveur',
    },
  ]);

  constructor() {
    forkJoin({
      resources: this.resourcesApi.getAll(),
      activities: this.activitiesApi.getAll(),
      schedules: this.schedulesApi.getAll(),
    })
      .pipe(
        switchMap((data) => {
          const latest = data.schedules[0];
          const detail$: Observable<ScheduleDetail | null> = latest
            ? this.schedulesApi.getById(latest.id)
            : of(null);
          return detail$.pipe(map((detail) => ({ ...data, detail })));
        }),
      )
      .subscribe({
        next: ({ resources, activities, schedules, detail }) => {
          this.resources.set(resources);
          this.activities.set(activities);
          this.schedules.set(schedules);
          this.latestDetail.set(detail);
          this.loading.set(false);
        },
        error: () => {
          this.error.set('Impossible de charger le tableau de bord.');
          this.loading.set(false);
        },
      });
  }

  formatMinutes(minutes: number): string {
    const h = Math.floor(minutes / 60);
    const m = Math.round(minutes % 60);
    return h === 0 ? `${m} min` : m === 0 ? `${h} h` : `${h} h ${m.toString().padStart(2, '0')}`;
  }

  relativeDate(iso: string): string {
    const diffMs = Date.now() - new Date(iso).getTime();
    const minutes = Math.round(diffMs / 60_000);
    if (minutes < 1) return "à l'instant";
    if (minutes < 60) return `il y a ${minutes} min`;
    const hours = Math.round(minutes / 60);
    if (hours < 24) return `il y a ${hours} h`;
    const days = Math.round(hours / 24);
    return days === 1 ? 'hier' : `il y a ${days} j`;
  }
}

function toMinutes(time: string): number {
  const [h, m] = time.split(':').map(Number);
  return h * 60 + m;
}
