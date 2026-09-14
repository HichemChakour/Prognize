/** Chemins SVG 24×24, trait 1.6 — un seul système d'icônes pour toute l'app. */
export const ICONS = {
  home: 'M3 11.5 12 4l9 7.5M5.5 10v10h13V10',
  list: 'M4 7h16M4 12h16M4 17h10',
  clock: 'M12 3.5a8.5 8.5 0 1 0 0 17 8.5 8.5 0 0 0 0-17zM12 7.5V12l3 2',
  calendar: 'M4.5 6h15v14h-15zM4.5 10.5h15M8 3.5V7M16 3.5V7',
  menu: 'M4 7h16M4 12h16M4 17h16',
  close: 'M6.5 6.5l11 11M17.5 6.5l-11 11',
  chevronDown: 'M6.5 9.5l5.5 5.5 5.5-5.5',
  chevronLeft: 'M14.5 6l-6 6 6 6',
  chevronRight: 'M9.5 6l6 6-6 6',
  sidebar: 'M4 5.5h16v13H4zM9 5.5v13',
  logout: 'M10 4.5H5.5v15H10M14.5 8.5l4 3.5-4 3.5M18.5 12H9.5',
  sun: 'M12 4.5v-1M12 20.5v-1M4.5 12h-1M20.5 12h-1M6.7 6.7l-.7-.7M18 18l-.7-.7M6.7 17.3l-.7.7M18 6l-.7.7M12 8a4 4 0 1 0 0 8 4 4 0 0 0 0-8z',
  moon: 'M20 14.5A8 8 0 0 1 9.5 4a8 8 0 1 0 10.5 10.5z',
  auto: 'M12 3.5a8.5 8.5 0 1 0 0 17V3.5z M12 3.5a8.5 8.5 0 0 1 0 17',
  plus: 'M12 5.5v13M5.5 12h13',
  check: 'M5 12.5l4.5 4.5L19 7.5',
  sparkle:
    'M12 3.5l1.8 5.2 5.2 1.8-5.2 1.8L12 17.5l-1.8-5.2L5 10.5l5.2-1.8zM19 16.5l.7 2 2 .7-2 .7-.7 2-.7-2-2-.7 2-.7z',
  pin: 'M12 3.5l2.6 5.3 5.9.9-4.3 4.1 1 5.8L12 16.9l-5.2 2.7 1-5.8-4.3-4.1 5.9-.9z',
  arrowLeft: 'M19 12H5.5M11 6l-6 6 6 6',
  trash: 'M5.5 7h13M9.5 7V4.5h5V7M7 7l.8 12.5h8.4L17 7M10 10.5v6M14 10.5v6',
  warning: 'M12 4.5 20.5 19H3.5zM12 10v4M12 16.8v.2',
} as const;

export type IconName = keyof typeof ICONS;
