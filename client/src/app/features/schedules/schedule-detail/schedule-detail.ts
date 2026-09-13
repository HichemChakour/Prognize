import { Component, computed, inject, input, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { Activities } from '../../../core/activities/activities';
import { Activity } from '../../../core/activities/activity.models';
import { Auth } from '../../../core/auth/auth';
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
}

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

  readonly id = input.required<string>();

  readonly schedule = signal<ScheduleDetailModel | null>(null);
  readonly activities = signal<Activity[]>([]);
  readonly resources = signal<Resource[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly weekStart = signal(startOfWeek(new Date()));
  readonly panel = signal<PanelState | { mode: 'solve' } | null>(null);

  readonly isAdmin = computed(() => this.auth.user()?.role === 'Admin');
  readonly hours = Array.from(
    { length: DAY_END_HOUR - DAY_START_HOUR },
    (_, i) => DAY_START_HOUR + i,
  );
  readonly dayHeight = (DAY_END_HOUR - DAY_START_HOUR) * HOUR_HEIGHT;
  readonly hourHeight = HOUR_HEIGHT;

  readonly days = computed(() => Array.from({ length: 7 }, (_, i) => addDays(this.weekStart(), i)));

  readonly weekLabel = computed(() => {
    const from = this.weekStart();
    const to = addDays(from, 6);
    const fmt = (d: Date) => d.toLocaleDateString('fr-FR', { day: 'numeric', month: 'short' });
    return `${fmt(from)} → ${fmt(to)} ${to.getFullYear()}`;
  });

  readonly placedByDay = computed<PlacedAssignment[][]>(() => {
    const schedule = this.schedule();
    if (!schedule) return this.days().map(() => []);

    return this.days().map((day) => {
      const dayKey = toDateInput(day);
      const ofDay = schedule.assignments
        .filter((a) => a.start.slice(0, 10) === dayKey)
        .sort((a, b) => a.start.localeCompare(b.start));
      return this.layout(ofDay);
    });
  });

  readonly conflictCount = computed(() => this.schedule()?.conflictCount ?? 0);

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

  shiftWeek(weeks: number): void {
    this.weekStart.set(addDays(this.weekStart(), weeks * 7));
  }

  goToday(): void {
    this.weekStart.set(startOfWeek(new Date()));
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

  private layout(assignments: Assignment[]): PlacedAssignment[] {
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
      return { assignment, top, height, lane, lanes: 1 };
    });
    const lanes = Math.max(1, laneEnds.length);
    return placed.map((p) => ({ ...p, lanes }));
  }
}
