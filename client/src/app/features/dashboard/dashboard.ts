import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { Activities } from '../../core/activities/activities';
import { Activity } from '../../core/activities/activity.models';
import { Auth } from '../../core/auth/auth';
import { Resource } from '../../core/resources/resource.models';
import { Resources } from '../../core/resources/resources';
import { ScheduleSummary } from '../../core/schedules/schedule.models';
import { Schedules } from '../../core/schedules/schedules';

interface KindCount {
  kind: string;
  count: number;
  ratio: number;
}

@Component({
  imports: [RouterLink],
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
    }).subscribe({
      next: ({ resources, activities, schedules }) => {
        this.resources.set(resources);
        this.activities.set(activities);
        this.schedules.set(schedules);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Impossible de charger le tableau de bord.');
        this.loading.set(false);
      },
    });
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
