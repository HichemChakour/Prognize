export type ResourceStatus = 'Active' | 'Inactive';

export type DayOfWeek =
  'Monday' | 'Tuesday' | 'Wednesday' | 'Thursday' | 'Friday' | 'Saturday' | 'Sunday';

export const DAYS_OF_WEEK: { value: DayOfWeek; label: string }[] = [
  { value: 'Monday', label: 'Lundi' },
  { value: 'Tuesday', label: 'Mardi' },
  { value: 'Wednesday', label: 'Mercredi' },
  { value: 'Thursday', label: 'Jeudi' },
  { value: 'Friday', label: 'Vendredi' },
  { value: 'Saturday', label: 'Samedi' },
  { value: 'Sunday', label: 'Dimanche' },
];

export interface AvailabilityWindow {
  day: DayOfWeek;
  start: string; // "08:00:00"
  end: string;
}

export interface Resource {
  id: string;
  name: string;
  kind: string;
  capacity: number | null;
  status: ResourceStatus;
  attributes: Record<string, unknown> | null;
  availability: AvailabilityWindow[];
  createdAt: string;
  updatedAt: string;
}

export interface CreateResourceRequest {
  name: string;
  kind: string;
  capacity: number | null;
  attributes: Record<string, unknown> | null;
  availability: AvailabilityWindow[];
}

export interface UpdateResourceRequest extends CreateResourceRequest {
  status: ResourceStatus;
}
