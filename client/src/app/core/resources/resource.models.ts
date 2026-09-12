export type ResourceStatus = 'Active' | 'Inactive';

export interface Resource {
  id: string;
  name: string;
  kind: string;
  capacity: number | null;
  status: ResourceStatus;
  attributes: Record<string, unknown> | null;
  createdAt: string;
  updatedAt: string;
}

export interface CreateResourceRequest {
  name: string;
  kind: string;
  capacity: number | null;
  attributes: Record<string, unknown> | null;
}

export interface UpdateResourceRequest extends CreateResourceRequest {
  status: ResourceStatus;
}
