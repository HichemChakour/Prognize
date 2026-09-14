import { DayOfWeek } from '../resources/resource.models';

export type ScheduleStatus = 'Draft' | 'Published' | 'Archived';

export const SCHEDULE_STATUS_LABELS: Record<ScheduleStatus, string> = {
  Draft: 'Brouillon',
  Published: 'Publié',
  Archived: 'Archivé',
};

export type ConflictType =
  | 'ResourceDoubleBooked'
  | 'ResourceUnavailable'
  | 'CapacityInsufficient'
  | 'RequirementUnmet'
  | 'DurationMismatch';

export interface Conflict {
  type: ConflictType;
  message: string;
  resourceId: string | null;
}

export interface AssignedResource {
  id: string;
  name: string;
  kind: string;
  capacity: number | null;
}

export interface Assignment {
  id: string;
  scheduleId: string;
  activityId: string;
  activityName: string;
  priority: number;
  start: string; // "2026-09-14T08:00:00" — heure locale, sans fuseau
  end: string;
  resources: AssignedResource[];
  conflicts: Conflict[];
}

export interface ScheduleSummary {
  id: string;
  name: string;
  status: ScheduleStatus;
  score: number | null;
  assignmentCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface ScheduleDetail {
  id: string;
  name: string;
  status: ScheduleStatus;
  score: number | null;
  metrics: SolveMetrics | null;
  assignments: Assignment[];
  conflictCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateScheduleRequest {
  name: string;
}

export interface UpdateScheduleRequest {
  name: string;
  status: ScheduleStatus;
}

export interface UpsertAssignmentRequest {
  activityId: string;
  start: string;
  end: string;
  resourceIds: string[];
}

export interface SolveRequest {
  weekStart: string; // "2026-09-14"
  days: DayOfWeek[];
  dayStart: string; // "08:00:00"
  dayEnd: string;
  timeLimitSeconds: number;
}

export interface SolveMetrics {
  outcome: 'Optimal' | 'Feasible' | 'Infeasible' | 'Unknown';
  activityCount: number;
  placedCount: number;
  unplacedCount: number;
  unplacedActivityIds: string[];
  wallTimeSeconds: number;
  branches: number;
  conflicts: number;
  solvedAt: string;
}
