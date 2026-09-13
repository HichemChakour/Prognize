import { DOCUMENT } from '@angular/common';
import { computed, effect, inject, Service, signal } from '@angular/core';

export type ThemeMode = 'system' | 'light' | 'dark';

const STORAGE_KEY = 'prognize.theme';
const ORDER: ThemeMode[] = ['system', 'light', 'dark'];

@Service()
export class Theme {
  private readonly document = inject(DOCUMENT);

  readonly mode = signal<ThemeMode>(this.restore());

  readonly label = computed(
    () =>
      ({ system: 'Thème : automatique', light: 'Thème : clair', dark: 'Thème : sombre' })[
        this.mode()
      ],
  );

  readonly icon = computed(() => ({ system: '◐', light: '☀', dark: '☾' })[this.mode()]);

  constructor() {
    effect(() => {
      const mode = this.mode();
      const root = this.document.documentElement;
      if (mode === 'system') {
        delete root.dataset['theme'];
      } else {
        root.dataset['theme'] = mode;
      }
      try {
        localStorage.setItem(STORAGE_KEY, mode);
      } catch {
        /* stockage indisponible : on garde l'état en mémoire */
      }
    });
  }

  cycle(): void {
    const next = ORDER[(ORDER.indexOf(this.mode()) + 1) % ORDER.length];
    this.mode.set(next);
  }

  private restore(): ThemeMode {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      return raw === 'light' || raw === 'dark' ? raw : 'system';
    } catch {
      return 'system';
    }
  }
}
