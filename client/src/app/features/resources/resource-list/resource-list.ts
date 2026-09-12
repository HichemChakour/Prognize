import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../core/auth/auth';
import { Resource } from '../../../core/resources/resource.models';
import { Resources } from '../../../core/resources/resources';

@Component({
  imports: [RouterLink],
  selector: 'app-resource-list',
  styleUrl: './resource-list.scss',
  templateUrl: './resource-list.html',
})
export class ResourceList {
  private readonly api = inject(Resources);
  private readonly auth = inject(Auth);

  readonly resources = signal<Resource[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly kindFilter = signal<string | null>(null);

  readonly isAdmin = computed(() => this.auth.user()?.role === 'Admin');

  readonly kinds = computed(() => [...new Set(this.resources().map((r) => r.kind))].sort());

  readonly filtered = computed(() => {
    const kind = this.kindFilter();
    return kind === null ? this.resources() : this.resources().filter((r) => r.kind === kind);
  });

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.getAll().subscribe({
      next: (list) => {
        this.resources.set(list);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.error.set('Erreur serveur, réessaie.');
      },
    });
  }

  remove(resource: Resource): void {
    if (!confirm(`Supprimer « ${resource.name} » ?`)) return;
    this.api.delete(resource.id).subscribe({ next: () => this.load() });
  }
}
