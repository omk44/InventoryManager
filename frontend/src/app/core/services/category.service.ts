import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { Category, CreateCategory, UpdateCategory } from '../models/category.model';

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/category`;

  getCategories(): Observable<ApiResponse<Category[]>> {
    return this.http.get<ApiResponse<Category[]>>(this.apiUrl);
  }

  getCategory(id: number): Observable<ApiResponse<Category>> {
    return this.http.get<ApiResponse<Category>>(`${this.apiUrl}/${id}`);
  }

  createCategory(data: CreateCategory): Observable<ApiResponse<Category>> {
    return this.http.post<ApiResponse<Category>>(this.apiUrl, data);
  }

  updateCategory(id: number, data: UpdateCategory): Observable<ApiResponse<Category>> {
    return this.http.put<ApiResponse<Category>>(`${this.apiUrl}/${id}`, data);
  }

  deleteCategory(id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/${id}`);
  }
}
