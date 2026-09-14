import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Activities } from '../../../core/activities/activities';
import { Activity } from '../../../core/activities/activity.models';
import { Auth } from '../../../core/auth/auth';

import { Icon } from '../../../shared/directives/icon';

@Component({
  imports: [RouterLink, Icon],
  selector: 'app-activity-list',
  styleUrl: './activity-list.scss',
  templateUrl: './activity-list.html',
})
export class ActivityList {
  private readonly api = inject(Activities);
  private readonly auth = inject(Auth);

  readonly activities = signal<Activity[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  readonly isAdmin = computed(() => this.auth.user()?.role === 'Admin');

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.getAll().subscribe({
      next: (list) => {
        this.activities.set(list);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Erreur serveur, réessaie.');
        this.loading.set(false);
      },
    });
  }

  remove(activity: Activity): void {
    if (!confirm(`Supprimer « ${activity.name} » ?`)) return;
    this.api.delete(activity.id).subscribe({ next: () => this.load() });
  }

  formatDuration(minutes: number): string {
    const h = Math.floor(minutes / 60);
    const m = minutes % 60;
    return h === 0 ? `${m} min` : m === 0 ? `${h} h` : `${h} h ${m.toString().padStart(2, '0')}`;
  }

  describeRequirement(r: Activity['requirements'][number]): string {
    if (r.minCapacity) return `${r.kind} ≥ ${r.minCapacity}`;
    return r.kind;
  }
}
