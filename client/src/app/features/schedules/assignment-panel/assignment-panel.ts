import { HttpErrorResponse } from '@angular/common/http';
import {
  Component,
  computed,
  effect,
  inject,
  input,
  output,
  signal,
  untracked,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Observable } from 'rxjs';
import { Activity } from '../../../core/activities/activity.models';
import { Resource } from '../../../core/resources/resource.models';
import { Assignment, Conflict } from '../../../core/schedules/schedule.models';
import { Schedules } from '../../../core/schedules/schedules';
import {
  addMinutes,
  fromInputs,
  toDateInput,
  toLocalIso,
  toTimeInput,
} from '../../../core/schedules/time';

export type PanelState = { mode: 'create'; start: Date } | { mode: 'edit'; assignment: Assignment };

import { Icon } from '../../../shared/directives/icon';

@Component({
  imports: [ReactiveFormsModule, Icon],
  selector: 'app-assignment-panel',
  styleUrl: './assignment-panel.scss',
  templateUrl: './assignment-panel.html',
})
export class AssignmentPanel {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(Schedules);

  readonly scheduleId = input.required<string>();
  readonly state = input.required<PanelState>();
  readonly activities = input.required<Activity[]>();
  readonly resources = input.required<Resource[]>();

  readonly saved = output<void>();
  readonly closed = output<void>();

  readonly form = this.fb.nonNullable.group({
    activityId: ['', Validators.required],
    date: ['', Validators.required],
    start: ['', Validators.required],
    end: ['', Validators.required],
  });

  readonly selectedIds = signal<Set<string>>(new Set());
  readonly saving = signal(false);
  readonly error = signal<string | null>(null);
  readonly conflicts = signal<Conflict[]>([]);

  readonly isEdit = computed(() => this.state().mode === 'edit');

  readonly activity = computed(() => {
    const id = this.activityIdSignal();
    return this.activities().find((a) => a.id === id) ?? null;
  });

  readonly resourcesByKind = computed(() => {
    const groups = new Map<string, Resource[]>();
    for (const r of this.resources()) {
      groups.set(r.kind, [...(groups.get(r.kind) ?? []), r]);
    }
    return [...groups.entries()].sort(([a], [b]) => a.localeCompare(b));
  });

  readonly requiredKinds = computed(
    () => new Set(this.activity()?.requirements.map((r) => r.kind) ?? []),
  );
  readonly pinnedIds = computed(
    () =>
      new Set(
        this.activity()?.requirements.flatMap((r) => (r.resourceId ? [r.resourceId] : [])) ?? [],
      ),
  );

  private readonly activityIdSignal = signal('');
  private resetting = false;

  constructor() {
    this.form.controls.activityId.valueChanges.subscribe((id) => {
      this.activityIdSignal.set(id);
      this.applyActivityDefaults();
    });
    this.form.controls.start.valueChanges.subscribe(() => this.syncEndFromDuration());

    effect(() => {
      const state = this.state();
      untracked(() => this.reset(state));
    });
  }

  conflictsFor(resourceId: string): Conflict[] {
    return this.conflicts().filter((c) => c.resourceId === resourceId);
  }

  isSelected(id: string): boolean {
    return this.selectedIds().has(id);
  }

  toggle(id: string): void {
    const next = new Set(this.selectedIds());
    next.has(id) ? next.delete(id) : next.add(id);
    this.selectedIds.set(next);
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const { activityId, date, start, end } = this.form.getRawValue();
    const request = {
      activityId,
      start: toLocalIso(fromInputs(date, start)),
      end: toLocalIso(fromInputs(date, end)),
      resourceIds: [...this.selectedIds()],
    };

    this.saving.set(true);
    this.error.set(null);

    const state = this.state();
    const call: Observable<Assignment> =
      state.mode === 'edit'
        ? this.api.updateAssignment(this.scheduleId(), state.assignment.id, request)
        : this.api.createAssignment(this.scheduleId(), request);

    call.subscribe({
      next: () => {
        this.saving.set(false);
        this.saved.emit();
      },
      error: (err: HttpErrorResponse) => {
        this.error.set(
          err.status === 400
            ? (err.error?.title ?? 'Données invalides.')
            : 'Erreur serveur, réessaie.',
        );
        this.saving.set(false);
      },
    });
  }

  remove(): void {
    const state = this.state();
    if (state.mode !== 'edit' || !confirm('Supprimer cette affectation ?')) return;
    this.api
      .deleteAssignment(this.scheduleId(), state.assignment.id)
      .subscribe({ next: () => this.saved.emit() });
  }

  private reset(state: PanelState): void {
    this.resetting = true;
    try {
      this.applyState(state);
    } finally {
      this.resetting = false;
    }
  }

  private applyState(state: PanelState): void {
    this.error.set(null);
    if (state.mode === 'create') {
      this.form.reset({
        activityId: '',
        date: toDateInput(state.start),
        start: toTimeInput(state.start),
        end: '',
      });
      this.selectedIds.set(new Set());
      this.conflicts.set([]);
      return;
    }
    const a = state.assignment;
    const start = new Date(a.start);
    const end = new Date(a.end);
    this.form.reset({
      activityId: a.activityId,
      date: toDateInput(start),
      start: toTimeInput(start),
      end: toTimeInput(end),
    });
    this.selectedIds.set(new Set(a.resources.map((r) => r.id)));
    this.conflicts.set(a.conflicts);
  }

  private applyActivityDefaults(): void {
    const activity = this.activity();
    if (!activity || this.resetting) return;
    this.syncEndFromDuration();
    if (this.state().mode === 'create') {
      this.selectedIds.set(new Set(this.pinnedIds()));
    }
  }

  private syncEndFromDuration(): void {
    if (this.resetting) return;
    const activity = this.activity();
    const { date, start } = this.form.getRawValue();
    if (!activity || !date || !start) return;
    const end = addMinutes(fromInputs(date, start), activity.durationMinutes);
    this.form.controls.end.setValue(toTimeInput(end), { emitEvent: false });
  }
}
