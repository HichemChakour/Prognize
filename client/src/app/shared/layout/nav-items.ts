export interface NavItem {
  path: string;
  label: string;
  /** chemin SVG 24x24, stroke */
  icon: string;
  exact?: boolean;
}

export const NAV_ITEMS: NavItem[] = [
  {
    path: '/',
    label: 'Tableau de bord',
    exact: true,
    icon: 'M3 12l9-8 9 8M5 10v10h14V10',
  },
  {
    path: '/resources',
    label: 'Ressources',
    icon: 'M4 6h16M4 12h16M4 18h10',
  },
  {
    path: '/activities',
    label: 'Activités',
    icon: 'M12 3a9 9 0 1 0 0 18 9 9 0 0 0 0-18zM12 7v5l3 2',
  },
  {
    path: '/schedules',
    label: 'Plannings',
    icon: 'M4 5h16v15H4zM4 10h16M8 3v4M16 3v4',
  },
];
