import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateResourceRequest, Resource, UpdateResourceRequest } from './resource.models';

@Service()
export class Resources {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/resources`;

  getAll(kind?: string): Observable<Resource[]> {
    let params = new HttpParams();
    if (kind) {
      params = params.set('kind', kind);
    }
    return this.http.get<Resource[]>(this.apiUrl, { params });
  }

  getById(id: string): Observable<Resource> {
    return this.http.get<Resource>(`${this.apiUrl}/${id}`);
  }

  create(request: CreateResourceRequest): Observable<Resource> {
    return this.http.post<Resource>(this.apiUrl, request);
  }

  update(id: string, request: UpdateResourceRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
