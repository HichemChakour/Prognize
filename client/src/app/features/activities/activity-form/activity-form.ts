import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, input, OnInit, signal } from '@angular/core';
import {
  FormArray,
  FormBuilder,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { forkJoin, Observable, of } from 'rxjs';
import { Activities } from '../../../core/activities/activities';
import {
  Activity,
  ActivityStatus,
  CreateActivityRequest,
} from '../../../core/activities/activity.models';
import { Resource } from '../../../core/resources/resource.models';
import { Resources } from '../../../core/resources/resources';

type RequirementGroup = FormGroup<{
  kind: FormControl<string>;
  resourceId: FormControl<string | null>;
  minCapacity: FormControl<number | null>;
}>;

import { Icon } from '../../../shared/directives/icon';

@Component({
  imports: [ReactiveFormsModule, RouterLink, Icon],
  selector: 'app-activity-form',
  styleUrl: './activity-form.scss',
  templateUrl: './activity-form.html',
})
export class ActivityForm implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(Activities);
  private readonly resourcesApi = inject(Resources);
  private readonly router = inject(Router);

  readonly id = input<string>();
  readonly isEdit = computed(() => this.id() !== undefined);

  readonly resources = signal<Resource[]>([]);
  readonly kinds = computed(() => [...new Set(this.resources().map((r) => r.kind))].sort());

  readonly requirements = new FormArray<RequirementGroup>([]);

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    durationMinutes: [60, [Validators.required, Validators.min(1), Validators.max(1440)]],
    priority: [3, [Validators.required, Validators.min(1), Validators.max(5)]],
    status: this.fb.nonNullable.control<ActivityStatus>('Active'),
    requirements: this.requirements,
  });

  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly error = signal<string | null>(null);

  ngOnInit(): void {
    const id = this.id();
    const activity$: Observable<Activity | null> = id ? this.api.getById(id) : of(null);

    forkJoin({ resources: this.resourcesApi.getAll(), activity: activity$ }).subscribe({
      next: ({ resources, activity }) => {
        this.resources.set(resources.filter((r) => r.status === 'Active'));
        if (activity) this.populate(activity);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Chargement impossible.');
        this.loading.set(false);
      },
    });
  }

  resourcesOfKind(kind: string): Resource[] {
    return this.resources().filter((r) => r.kind === kind);
  }

  addRequirement(
    kind = '',
    resourceId: string | null = null,
    minCapacity: number | null = null,
  ): void {
    const group: RequirementGroup = this.fb.group({
      kind: this.fb.nonNullable.control(kind, Validators.required),
      resourceId: this.fb.control<string | null>(resourceId),
      minCapacity: this.fb.control<number | null>(minCapacity, Validators.min(1)),
    });

    group.controls.kind.valueChanges.subscribe(() => group.controls.resourceId.setValue(null));

    this.requirements.push(group);
  }

  removeRequirement(index: number): void {
    this.requirements.removeAt(index);
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.error.set(null);

    const value = this.form.getRawValue();
    const request: CreateActivityRequest = {
      name: value.name,
      durationMinutes: value.durationMinutes,
      priority: value.priority,
      requirements: this.requirements.getRawValue().map((r) => ({
        kind: r.kind,
        resourceId: r.resourceId || null,
        minCapacity: r.minCapacity,
      })),
    };

    const id = this.id();
    const call: Observable<unknown> = id
      ? this.api.update(id, { ...request, status: value.status })
      : this.api.create(request);

    call.subscribe({
      next: () => this.router.navigate(['/activities']),
      error: (err: HttpErrorResponse) => {
        this.error.set(
          err.status === 409
            ? 'Une activité porte déjà ce nom.'
            : err.status === 400
              ? (err.error?.title ?? 'Données invalides.')
              : 'Erreur serveur, réessaie.',
        );
        this.saving.set(false);
      },
    });
  }

  private populate(activity: Activity): void {
    this.form.patchValue({
      name: activity.name,
      durationMinutes: activity.durationMinutes,
      priority: activity.priority,
      status: activity.status,
    });

    this.requirements.clear();
    for (const r of activity.requirements) {
      this.addRequirement(r.kind, r.resourceId, r.minCapacity);
    }
  }
}
