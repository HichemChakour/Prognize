import { Directive, effect, ElementRef, inject, input } from '@angular/core';

/** Anime un nombre de 0 à sa valeur. Usage : <span [appCountUp]="42"></span> */
@Directive({ selector: '[appCountUp]' })
export class CountUp {
  private readonly el = inject<ElementRef<HTMLElement>>(ElementRef);

  readonly appCountUp = input.required<number>();
  readonly suffix = input('');

  private frame = 0;

  constructor() {
    effect(() => {
      const target = this.appCountUp();
      cancelAnimationFrame(this.frame);

      const reduced = window.matchMedia?.('(prefers-reduced-motion: reduce)').matches;
      if (reduced || target === 0) {
        this.render(target);
        return;
      }

      const duration = 900;
      const start = performance.now();
      const tick = (now: number) => {
        const t = Math.min(1, (now - start) / duration);
        const eased = 1 - Math.pow(1 - t, 3);
        this.render(Math.round(target * eased));
        if (t < 1) this.frame = requestAnimationFrame(tick);
      };
      this.frame = requestAnimationFrame(tick);
    });
  }

  private render(value: number): void {
    this.el.nativeElement.textContent = `${value}${this.suffix()}`;
  }
}
