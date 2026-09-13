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
import { Observable } from 'rxjs';
import {
  CreateResourceRequest,
  DAYS_OF_WEEK,
  DayOfWeek,
  Resource,
  ResourceStatus,
} from '../../../core/resources/resource.models';
import { Resources } from '../../../core/resources/resources';

type AttributeGroup = FormGroup<{ key: FormControl<string>; value: FormControl<string> }>;
type WindowGroup = FormGroup<{
  day: FormControl<DayOfWeek>;
  start: FormControl<string>;
  end: FormControl<string>;
}>;

const KIND_SUGGESTIONS = ['room', 'teacher', 'class', 'equipment'];

@Component({
  imports: [ReactiveFormsModule, RouterLink],
  selector: 'app-resource-form',
  styleUrl: './resource-form.scss',
  templateUrl: './resource-form.html',
})
export class ResourceForm implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(Resources);
  private readonly router = inject(Router);

  readonly id = input<string>();

  readonly isEdit = computed(() => this.id() !== undefined);
  readonly kindSuggestions = KIND_SUGGESTIONS;
  readonly days = DAYS_OF_WEEK;

  readonly attributes = new FormArray<AttributeGroup>([]);
  readonly availability = new FormArray<WindowGroup>([]);

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    kind: ['', [Validators.required, Validators.maxLength(50)]],
    capacity: this.fb.control<number | null>(null, [Validators.min(1)]),
    status: this.fb.nonNullable.control<ResourceStatus>('Active'),
    attributes: this.attributes,
    availability: this.availability,
  });

  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly error = signal<string | null>(null);

  ngOnInit(): void {
    const id = this.id();
    if (!id) return;

    this.loading.set(true);
    this.api.getById(id).subscribe({
      next: (resource) => {
        this.populate(resource);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Ressource introuvable.');
        this.loading.set(false);
      },
    });
  }

  addAttribute(key = '', value = ''): void {
    this.attributes.push(
      this.fb.nonNullable.group({
        key: [key, Validators.required],
        value: [value],
      }),
    );
  }

  removeAttribute(index: number): void {
    this.attributes.removeAt(index);
  }

  addWindow(day: DayOfWeek = 'Monday', start = '08:00', end = '12:00'): void {
    this.availability.push(
      this.fb.nonNullable.group({
        day: [day],
        start: [start, Validators.required],
        end: [end, Validators.required],
      }),
    );
  }

  removeWindow(index: number): void {
    this.availability.removeAt(index);
  }

  windowInvalid(group: WindowGroup): boolean {
    const { start, end } = group.getRawValue();
    return start !== '' && end !== '' && end <= start;
  }

  submit(): void {
    if (this.form.invalid || this.availability.controls.some((w) => this.windowInvalid(w))) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.error.set(null);

    const value = this.form.getRawValue();
    const request: CreateResourceRequest = {
      name: value.name,
      kind: value.kind,
      capacity: value.capacity,
      attributes: this.toAttributesObject(),
      availability: this.availability.getRawValue().map((w) => ({
        day: w.day,
        start: `${w.start}:00`,
        end: `${w.end}:00`,
      })),
    };

    const id = this.id();
    const call: Observable<unknown> = id
      ? this.api.update(id, { ...request, status: value.status })
      : this.api.create(request);

    call.subscribe({
      next: () => this.router.navigate(['/resources']),
      error: (err: HttpErrorResponse) => {
        this.error.set(
          err.status === 409
            ? 'Une ressource de ce type porte déjà ce nom.'
            : err.status === 403
              ? "Vous n'avez pas les droits pour cette action."
              : 'Erreur serveur, réessaie.',
        );
        this.saving.set(false);
      },
    });
  }

  private populate(resource: Resource): void {
    this.form.patchValue({
      name: resource.name,
      kind: resource.kind,
      capacity: resource.capacity,
      status: resource.status,
    });

    this.attributes.clear();
    for (const [key, value] of Object.entries(resource.attributes ?? {})) {
      this.addAttribute(key, String(value));
    }

    this.availability.clear();
    for (const w of resource.availability) {
      this.addWindow(w.day, w.start.slice(0, 5), w.end.slice(0, 5));
    }
  }

  private toAttributesObject(): Record<string, unknown> | null {
    const entries = this.attributes
      .getRawValue()
      .filter((a) => a.key.trim() !== '')
      .map((a) => [a.key.trim(), this.parseValue(a.value)] as const);

    return entries.length ? Object.fromEntries(entries) : null;
  }

  private parseValue(raw: string): unknown {
    const value = raw.trim();
    if (value === 'true') return true;
    if (value === 'false') return false;
    if (value !== '' && !Number.isNaN(Number(value))) return Number(value);
    return value;
  }
}
