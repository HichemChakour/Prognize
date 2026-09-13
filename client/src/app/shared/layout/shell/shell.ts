import { Component, computed, effect, HostListener, inject, signal } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { Auth } from '../../../core/auth/auth';
import { Viewport } from '../../../core/layout/viewport';
import { Theme } from '../../../core/theme/theme';
import { NAV_ITEMS } from '../nav-items';

const COLLAPSED_KEY = 'prognize.sidebar.collapsed';

@Component({
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  selector: 'app-shell',
  styleUrl: './shell.scss',
  templateUrl: './shell.html',
})
export class Shell {
  protected readonly auth = inject(Auth);
  protected readonly theme = inject(Theme);
  protected readonly viewport = inject(Viewport);
  private readonly router = inject(Router);

  readonly items = NAV_ITEMS;

  /** Sidebar repliée en icônes (desktop). Persisté. */
  readonly collapsed = signal(this.restoreCollapsed());
  /** Drawer mobile ouvert. */
  readonly drawerOpen = signal(false);
  readonly userMenuOpen = signal(false);

  /** Sur tablette la sidebar est toujours en icônes, sur desktop selon le choix. */
  readonly isRail = computed(
    () => this.viewport.isTablet() || (this.viewport.isDesktop() && this.collapsed()),
  );

  readonly initials = computed(() => {
    const name = this.auth.user()?.displayName ?? '';
    return name
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map((p) => p[0]!.toUpperCase())
      .join('');
  });

  readonly pageTitle = signal('');

  constructor() {
    this.router.events.pipe(filter((e) => e instanceof NavigationEnd)).subscribe(() => {
      this.drawerOpen.set(false);
      this.userMenuOpen.set(false);
      this.pageTitle.set(this.resolveTitle(this.router.url));
    });
    this.pageTitle.set(this.resolveTitle(this.router.url));

    effect(() => {
      try {
        localStorage.setItem(COLLAPSED_KEY, String(this.collapsed()));
      } catch {
        /* ignore */
      }
    });

    effect(() => {
      document.body.classList.toggle('drawer-open', this.drawerOpen() && this.viewport.isMobile());
    });
  }

  toggleCollapsed(): void {
    this.collapsed.update((c) => !c);
  }

  toggleDrawer(): void {
    this.drawerOpen.update((o) => !o);
  }

  toggleUserMenu(event: Event): void {
    event.stopPropagation();
    this.userMenuOpen.update((o) => !o);
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    this.drawerOpen.set(false);
    this.userMenuOpen.set(false);
  }

  @HostListener('document:click')
  onDocumentClick(): void {
    this.userMenuOpen.set(false);
  }

  private resolveTitle(url: string): string {
    const match = this.items
      .filter((i) => (i.exact ? url === i.path : url.startsWith(i.path)))
      .sort((a, b) => b.path.length - a.path.length)[0];
    return match?.label ?? 'Prognize';
  }

  private restoreCollapsed(): boolean {
    try {
      return localStorage.getItem(COLLAPSED_KEY) === 'true';
    } catch {
      return false;
    }
  }
}
