import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../core/auth/auth';
import { SCHEDULE_STATUS_LABELS, ScheduleSummary } from '../../../core/schedules/schedule.models';
import { Schedules } from '../../../core/schedules/schedules';
import { Icon } from '../../../shared/directives/icon';

@Component({
  imports: [RouterLink, ReactiveFormsModule, Icon],
  selector: 'app-schedule-list',
  styleUrl: './schedule-list.scss',
  templateUrl: './schedule-list.html',
})
export class ScheduleList {
  readonly statusLabels = SCHEDULE_STATUS_LABELS;
  private readonly api = inject(Schedules);
  private readonly auth = inject(Auth);
  private readonly fb = inject(FormBuilder);

  readonly schedules = signal<ScheduleSummary[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly creating = signal(false);

  readonly isAdmin = computed(() => this.auth.user()?.role === 'Admin');

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
  });

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.api.getAll().subscribe({
      next: (list) => {
        this.schedules.set(list);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Erreur serveur, réessaie.');
        this.loading.set(false);
      },
    });
  }

  create(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.creating.set(true);
    this.api.create({ name: this.form.getRawValue().name.trim() }).subscribe({
      next: () => {
        this.form.reset();
        this.creating.set(false);
        this.load();
      },
      error: () => this.creating.set(false),
    });
  }

  remove(schedule: ScheduleSummary): void {
    if (
      !confirm(
        `Supprimer le planning « ${schedule.name} » et ses ${schedule.assignmentCount} affectation(s) ?`,
      )
    )
      return;
    this.api.delete(schedule.id).subscribe({ next: () => this.load() });
  }
}
