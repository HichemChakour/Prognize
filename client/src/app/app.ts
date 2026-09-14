import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Theme } from './core/theme/theme';

@Component({
  imports: [RouterOutlet],
  selector: 'app-root',
  styleUrl: './app.scss',
  templateUrl: './app.html',
})
export class App {
  /** Instancié ici pour que le thème s'applique aussi hors du Shell (login, inscription). */
  private readonly theme = inject(Theme);
}
