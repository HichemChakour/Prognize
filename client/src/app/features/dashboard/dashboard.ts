import { Component, inject } from '@angular/core';
import { Auth } from '../../core/auth/auth';

@Component({
  imports: [],
  selector: 'app-dashboard',
  styleUrl: './dashboard.scss',
  templateUrl: './dashboard.html',
})
export class Dashboard {
  protected readonly auth = inject(Auth);
}
