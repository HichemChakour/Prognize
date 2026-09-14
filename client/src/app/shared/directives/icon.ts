import { Component, input } from '@angular/core';
import { ICONS, IconName } from '../icons';

/** <app-icon name="calendar" /> — SVG inline, hérite de la couleur du texte. */
@Component({
  selector: 'app-icon',
  template: `<svg viewBox="0 0 24 24" aria-hidden="true"><path [attr.d]="path()" /></svg>`,
  styles: `
    :host {
      display: inline-flex;
      width: 1em;
      height: 1em;
      line-height: 0;
      flex: none;
    }
    svg {
      width: 100%;
      height: 100%;
      fill: none;
      stroke: currentColor;
      stroke-width: 1.6;
      stroke-linecap: round;
      stroke-linejoin: round;
    }
  `,
})
export class Icon {
  readonly name = input.required<IconName>();
  readonly path = () => ICONS[this.name()];
}
