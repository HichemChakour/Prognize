import { IconName } from '../icons';

export interface NavItem {
  path: string;
  label: string;
  icon: IconName;
  exact?: boolean;
}

export const NAV_ITEMS: NavItem[] = [
  {
    path: '/',
    label: 'Tableau de bord',
    exact: true,
    icon: 'home',
  },
  {
    path: '/resources',
    label: 'Ressources',
    icon: 'list',
  },
  {
    path: '/activities',
    label: 'Activités',
    icon: 'clock',
  },
  {
    path: '/schedules',
    label: 'Plannings',
    icon: 'calendar',
  },
];
