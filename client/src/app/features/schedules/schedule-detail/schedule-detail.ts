import { Component, computed, inject, input, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { Activities } from '../../../core/activities/activities';
import { Activity } from '../../../core/activities/activity.models';
import { Auth } from '../../../core/auth/auth';
import { Viewport } from '../../../core/layout/viewport';
import { Resource } from '../../../core/resources/resource.models';
import { Resources } from '../../../core/resources/resources';
import {
  Assignment,
  ScheduleDetail as ScheduleDetailModel,
} from '../../../core/schedules/schedule.models';
import { Schedules } from '../../../core/schedules/schedules';
import {
  addDays,
  addMinutes,
  minutesOfDay,
  startOfWeek,
  toDateInput,
} from '../../../core/schedules/time';
import { AssignmentPanel, PanelState } from '../assignment-panel/assignment-panel';
import { SolvePanel } from '../solve-panel/solve-panel';

export const DAY_START_HOUR = 7;
export const DAY_END_HOUR = 20;
export const HOUR_HEIGHT = 56;

interface PlacedAssignment {
  assignment: Assignment;
  top: number;
  height: number;
  lane: number;
  lanes: number;
  color: string;
  dimmed: boolean;
}

interface LegendEntry {
  id: string;
  name: string;
  color: string;
}

const SERIES_COUNT = 6;

@Component({
  imports: [RouterLink, AssignmentPanel, SolvePanel],
  selector: 'app-schedule-detail',
  styleUrl: './schedule-detail.scss',
  templateUrl: './schedule-detail.html',
})
export class ScheduleDetail implements OnInit {
  private readonly api = inject(Schedules);
  private readonly activitiesApi = inject(Activities);
  private readonly resourcesApi = inject(Resources);
  private readonly auth = inject(Auth);
  private readonly viewport = inject(Viewport);

  readonly id = input.required<string>();

  readonly schedule = signal<ScheduleDetailModel | null>(null);
  readonly activities = signal<Activity[]>([]);
  readonly resources = signal<Resource[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly weekStart = signal(startOfWeek(new Date()));
  readonly dayIndex = signal((new Date().getDay() + 6) % 7);
  readonly isMobile = this.viewport.isMobile;
  readonly panel = signal<PanelState | { mode: 'solve' } | null>(null);

  /** Type de ressource qui pilote la couleur des blocs (null = premier type disponible). */
  readonly colorKind = signal<string | null>(null);
  /** Ressource mise en évidence via la légende (les autres blocs sont estompés). */
  readonly highlightId = signal<string | null>(null);

  readonly isAdmin = computed(() => this.auth.user()?.role === 'Admin');
  readonly hours = Array.from(
    { length: DAY_END_HOUR - DAY_START_HOUR },
    (_, i) => DAY_START_HOUR + i,
  );
  readonly dayHeight = (DAY_END_HOUR - DAY_START_HOUR) * HOUR_HEIGHT;
  readonly hourHeight = HOUR_HEIGHT;

  readonly days = computed(() => Array.from({ length: 7 }, (_, i) => addDays(this.weekStart(), i)));

  /** Semaine complète sur desktop, un seul jour sur mobile. */
  readonly visibleDays = computed(() =>
    this.isMobile() ? [this.days()[this.dayIndex()]] : this.days(),
  );

  readonly weekLabel = computed(() => {
    if (this.isMobile()) {
      const d = this.days()[this.dayIndex()];
      return d.toLocaleDateString('fr-FR', { weekday: 'long', day: 'numeric', month: 'short' });
    }
    const from = this.weekStart();
    const to = addDays(from, 6);
    const fmt = (d: Date) => d.toLocaleDateString('fr-FR', { day: 'numeric', month: 'short' });
    return `${fmt(from)} → ${fmt(to)} ${to.getFullYear()}`;
  });

  readonly placedByDay = computed<PlacedAssignment[][]>(() => {
    const schedule = this.schedule();
    if (!schedule) return this.visibleDays().map(() => []);

    return this.visibleDays().map((day) => {
      const dayKey = toDateInput(day);
      const ofDay = schedule.assignments
        .filter((a) => a.start.slice(0, 10) === dayKey)
        .sort((a, b) => a.start.localeCompare(b.start));
      return this.layout(ofDay, this.colorById(), this.activeColorKind(), this.highlightId());
    });
  });

  readonly conflictCount = computed(() => this.schedule()?.conflictCount ?? 0);

  readonly kinds = computed(() => [...new Set(this.resources().map((r) => r.kind))].sort());
  readonly activeColorKind = computed(() => this.colorKind() ?? this.kinds()[0] ?? null);

  /** Couleur stable par ressource du type choisi : ordre alphabétique → slot 1..6, au-delà « autres ». */
  readonly legend = computed<LegendEntry[]>(() => {
    const kind = this.activeColorKind();
    if (!kind) return [];
    return this.resources()
      .filter((r) => r.kind === kind)
      .sort((a, b) => a.name.localeCompare(b.name))
      .map((r, i) => ({
        id: r.id,
        name: r.name,
        color: i < SERIES_COUNT ? `var(--series-${i + 1})` : 'var(--series-other)',
      }));
  });

  private readonly colorById = computed(() => new Map(this.legend().map((e) => [e.id, e.color])));

  readonly assignmentPanel = computed<PanelState | null>(() => {
    const p = this.panel();
    return p && p.mode !== 'solve' ? p : null;
  });

  readonly solvePanelOpen = computed(() => this.panel()?.mode === 'solve');

  readonly unplacedActivities = computed(() => {
    const ids = this.schedule()?.metrics?.unplacedActivityIds ?? [];
    const byId = new Map(this.activities().map((a) => [a.id, a.name]));
    return ids.map((id) => byId.get(id) ?? 'Activité inactive');
  });

  readonly selectedAssignmentId = computed(() => {
    const p = this.panel();
    return p?.mode === 'edit' ? p.assignment.id : null;
  });

  ngOnInit(): void {
    forkJoin({
      schedule: this.api.getById(this.id()),
      activities: this.activitiesApi.getAll(),
      resources: this.resourcesApi.getAll(),
    }).subscribe({
      next: ({ schedule, activities, resources }) => {
        this.schedule.set(schedule);
        this.activities.set(activities.filter((a) => a.status === 'Active'));
        this.resources.set(resources.filter((r) => r.status === 'Active'));
        if (schedule.assignments.length > 0) {
          this.weekStart.set(startOfWeek(new Date(schedule.assignments[0].start)));
        }
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Planning introuvable.');
        this.loading.set(false);
      },
    });
  }

  reload(): void {
    this.api.getById(this.id()).subscribe({ next: (s) => this.schedule.set(s) });
  }

  /** Sur mobile on avance d'un jour, sur desktop d'une semaine. */
  shift(direction: 1 | -1): void {
    if (!this.isMobile()) {
      this.weekStart.set(addDays(this.weekStart(), direction * 7));
      return;
    }
    const next = this.dayIndex() + direction;
    if (next < 0) {
      this.weekStart.set(addDays(this.weekStart(), -7));
      this.dayIndex.set(6);
    } else if (next > 6) {
      this.weekStart.set(addDays(this.weekStart(), 7));
      this.dayIndex.set(0);
    } else {
      this.dayIndex.set(next);
    }
  }

  goToday(): void {
    const today = new Date();
    this.weekStart.set(startOfWeek(today));
    this.dayIndex.set((today.getDay() + 6) % 7);
  }

  isToday(day: Date): boolean {
    return toDateInput(day) === toDateInput(new Date());
  }

  openCreate(day: Date, event: MouseEvent): void {
    if (!this.isAdmin()) return;
    const column = event.currentTarget as HTMLElement;
    const y = event.clientY - column.getBoundingClientRect().top;
    const minutes = Math.floor(((y / HOUR_HEIGHT) * 60) / 30) * 30;
    const start = addMinutes(
      new Date(day.getFullYear(), day.getMonth(), day.getDate(), DAY_START_HOUR),
      minutes,
    );
    this.panel.set({ mode: 'create', start });
  }

  openSolve(): void {
    this.panel.set({ mode: 'solve' });
  }

  onSolved(detail: ScheduleDetailModel): void {
    this.panel.set(null);
    this.schedule.set(detail);
    if (detail.assignments.length > 0) {
      this.weekStart.set(startOfWeek(new Date(detail.assignments[0].start)));
    }
  }

  openEdit(assignment: Assignment, event: Event): void {
    event.stopPropagation();
    this.panel.set({ mode: 'edit', assignment });
  }

  closePanel(): void {
    this.panel.set(null);
  }

  onSaved(): void {
    this.panel.set(null);
    this.reload();
  }

  setColorKind(kind: string): void {
    this.colorKind.set(kind);
    this.highlightId.set(null);
  }

  toggleHighlight(id: string): void {
    this.highlightId.update((current) => (current === id ? null : id));
  }

  private layout(
    assignments: Assignment[],
    colorById: Map<string, string>,
    kind: string | null,
    highlightId: string | null,
  ): PlacedAssignment[] {
    const laneEnds: number[] = [];
    const placed = assignments.map((assignment) => {
      const start = minutesOfDay(new Date(assignment.start));
      const end = minutesOfDay(new Date(assignment.end));
      let lane = laneEnds.findIndex((laneEnd) => laneEnd <= start);
      if (lane === -1) {
        lane = laneEnds.length;
        laneEnds.push(end);
      } else {
        laneEnds[lane] = end;
      }
      const top = ((start - DAY_START_HOUR * 60) / 60) * HOUR_HEIGHT;
      const height = Math.max(((end - start) / 60) * HOUR_HEIGHT, 18);
      const pivot = assignment.resources.find((r) => r.kind === kind);
      const color = (pivot && colorById.get(pivot.id)) ?? 'var(--series-other)';
      const dimmed = highlightId !== null && pivot?.id !== highlightId;
      return { assignment, top, height, lane, lanes: 1, color, dimmed };
    });
    const lanes = Math.max(1, laneEnds.length);
    return placed.map((p) => ({ ...p, lanes }));
  }
}
