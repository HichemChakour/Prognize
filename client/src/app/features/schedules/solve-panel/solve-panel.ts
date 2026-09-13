import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, input, output, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { DAYS_OF_WEEK, DayOfWeek } from '../../../core/resources/resource.models';
import { ScheduleDetail } from '../../../core/schedules/schedule.models';
import { Schedules } from '../../../core/schedules/schedules';
import { toDateInput } from '../../../core/schedules/time';

@Component({
  imports: [ReactiveFormsModule],
  selector: 'app-solve-panel',
  styleUrl: './solve-panel.scss',
  templateUrl: './solve-panel.html',
})
export class SolvePanel {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(Schedules);

  readonly scheduleId = input.required<string>();
  readonly weekStart = input.required<Date>();
  readonly hasAssignments = input(false);

  readonly solved = output<ScheduleDetail>();
  readonly closed = output<void>();

  readonly days = DAYS_OF_WEEK;
  readonly selectedDays = signal<Set<DayOfWeek>>(
    new Set(['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday']),
  );

  readonly form = this.fb.nonNullable.group({
    dayStart: ['08:00', Validators.required],
    dayEnd: ['18:00', Validators.required],
    timeLimitSeconds: [10, [Validators.min(1), Validators.max(120)]],
  });

  readonly running = signal(false);
  readonly error = signal<string | null>(null);

  isDaySelected(day: DayOfWeek): boolean {
    return this.selectedDays().has(day);
  }

  toggleDay(day: DayOfWeek): void {
    const next = new Set(this.selectedDays());
    next.has(day) ? next.delete(day) : next.add(day);
    this.selectedDays.set(next);
  }

  run(): void {
    if (this.form.invalid || this.selectedDays().size === 0) return;
    if (
      this.hasAssignments() &&
      !confirm('Les affectations actuelles seront remplacées. Continuer ?')
    )
      return;

    const { dayStart, dayEnd, timeLimitSeconds } = this.form.getRawValue();
    this.running.set(true);
    this.error.set(null);

    this.api
      .solve(this.scheduleId(), {
        weekStart: toDateInput(this.weekStart()),
        days: this.days.map((d) => d.value).filter((d) => this.selectedDays().has(d)),
        dayStart: `${dayStart}:00`,
        dayEnd: `${dayEnd}:00`,
        timeLimitSeconds,
      })
      .subscribe({
        next: (detail) => {
          this.running.set(false);
          this.solved.emit(detail);
        },
        error: (err: HttpErrorResponse) => {
          this.error.set(
            err.status === 400
              ? (err.error?.title ?? 'Paramètres invalides.')
              : 'Le solveur a échoué.',
          );
          this.running.set(false);
        },
      });
  }
}
