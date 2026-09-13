import { Service, signal } from '@angular/core';

const MOBILE_QUERY = '(max-width: 767px)';

@Service()
export class Viewport {
  readonly isMobile = signal(false);

  constructor() {
    if (typeof window === 'undefined' || !window.matchMedia) return;
    const media = window.matchMedia(MOBILE_QUERY);
    this.isMobile.set(media.matches);
    media.addEventListener('change', (e) => this.isMobile.set(e.matches));
  }
}
