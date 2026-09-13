import { computed, Service, signal } from '@angular/core';

@Service()
export class Viewport {
  /** < 768px */
  readonly isMobile = signal(false);
  /** 768px – 1023px */
  readonly isTablet = signal(false);
  readonly isDesktop = computed(() => !this.isMobile() && !this.isTablet());

  constructor() {
    if (typeof window === 'undefined' || !window.matchMedia) return;
    this.bind('(max-width: 767px)', this.isMobile);
    this.bind('(min-width: 768px) and (max-width: 1023px)', this.isTablet);
  }

  private bind(query: string, target: { set(value: boolean): void }): void {
    const media = window.matchMedia(query);
    target.set(media.matches);
    media.addEventListener('change', (e) => target.set(e.matches));
  }
}
