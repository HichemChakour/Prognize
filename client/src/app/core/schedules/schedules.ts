import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  Assignment,
  CreateScheduleRequest,
  ScheduleDetail,
  ScheduleSummary,
  SolveRequest,
  UpdateScheduleRequest,
  UpsertAssignmentRequest,
} from './schedule.models';

@Service()
export class Schedules {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/schedules`;

  getAll(): Observable<ScheduleSummary[]> {
    return this.http.get<ScheduleSummary[]>(this.baseUrl);
  }

  getById(id: string): Observable<ScheduleDetail> {
    return this.http.get<ScheduleDetail>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateScheduleRequest): Observable<ScheduleSummary> {
    return this.http.post<ScheduleSummary>(this.baseUrl, request);
  }

  update(id: string, request: UpdateScheduleRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  createAssignment(scheduleId: string, request: UpsertAssignmentRequest): Observable<Assignment> {
    return this.http.post<Assignment>(`${this.baseUrl}/${scheduleId}/assignments`, request);
  }

  updateAssignment(
    scheduleId: string,
    id: string,
    request: UpsertAssignmentRequest,
  ): Observable<Assignment> {
    return this.http.put<Assignment>(`${this.baseUrl}/${scheduleId}/assignments/${id}`, request);
  }

  deleteAssignment(scheduleId: string, id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${scheduleId}/assignments/${id}`);
  }

  solve(id: string, request: SolveRequest): Observable<ScheduleDetail> {
    return this.http.post<ScheduleDetail>(`${this.baseUrl}/${id}/solve`, request);
  }
}
