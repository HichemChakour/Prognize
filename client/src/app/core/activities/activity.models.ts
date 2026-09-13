export type ActivityStatus = 'Active' | 'Inactive';

export interface ResourceRequirement {
  kind: string;
  resourceId: string | null;
  minCapacity: number | null;
}

export interface Activity {
  id: string;
  name: string;
  durationMinutes: number;
  priority: number;
  requirements: ResourceRequirement[];
  status: ActivityStatus;
  createdAt: string;
  updatedAt: string;
}

export interface CreateActivityRequest {
  name: string;
  durationMinutes: number;
  priority: number;
  requirements: ResourceRequirement[];
}

export interface UpdateActivityRequest extends CreateActivityRequest {
  status: ActivityStatus;
}
