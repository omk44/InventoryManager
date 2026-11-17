import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PagedResult } from '../models/api-response.model';
import { CreateTaskItem, TaskFilterParams, TaskItem, UpdateTaskItem } from '../models/task.model';

@Injectable({
  providedIn: 'root'
})
export class TaskService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/tasks`;

  getTasks(filterParams: TaskFilterParams): Observable<ApiResponse<PagedResult<TaskItem>>> {
    let params = new HttpParams()
      .set('pageNumber', filterParams.pageNumber.toString())
      .set('pageSize', filterParams.pageSize.toString());

    if (filterParams.searchTerm && filterParams.searchTerm.trim() !== '') {
      params = params.set('searchTerm', filterParams.searchTerm.trim());
    }

    if (filterParams.status) {
      params = params.set('status', filterParams.status);
    }

    if (filterParams.priority) {
      params = params.set('priority', filterParams.priority);
    }

    return this.http.get<ApiResponse<PagedResult<TaskItem>>>(this.apiUrl, { params });
  }

  getTask(id: number): Observable<ApiResponse<TaskItem>> {
    return this.http.get<ApiResponse<TaskItem>>(`${this.apiUrl}/${id}`);
  }

  createTask(data: CreateTaskItem): Observable<ApiResponse<TaskItem>> {
    return this.http.post<ApiResponse<TaskItem>>(this.apiUrl, data);
  }

  updateTask(id: number, data: UpdateTaskItem): Observable<ApiResponse<TaskItem>> {
    return this.http.put<ApiResponse<TaskItem>>(`${this.apiUrl}/${id}`, data);
  }

  deleteTask(id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/${id}`);
  }
}
