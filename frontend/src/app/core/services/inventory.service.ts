import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PagedResult } from '../models/api-response.model';
import { CreateInventoryItem, InventoryFilterParams, InventoryItem, UpdateInventoryItem } from '../models/inventory.model';

@Injectable({
  providedIn: 'root'
})
export class InventoryService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/inventory`;

  getItems(filterParams: InventoryFilterParams): Observable<ApiResponse<PagedResult<InventoryItem>>> {
    let params = new HttpParams()
      .set('pageNumber', filterParams.pageNumber.toString())
      .set('pageSize', filterParams.pageSize.toString());

    if (filterParams.searchTerm && filterParams.searchTerm.trim() !== '') {
      params = params.set('searchTerm', filterParams.searchTerm.trim());
    }

    if (filterParams.categoryId !== undefined && filterParams.categoryId !== '') {
      params = params.set('categoryId', filterParams.categoryId.toString());
    }

    if (filterParams.lowStockOnly !== undefined) {
      params = params.set('lowStockOnly', filterParams.lowStockOnly.toString());
    }

    return this.http.get<ApiResponse<PagedResult<InventoryItem>>>(this.apiUrl, { params });
  }

  getItem(id: number): Observable<ApiResponse<InventoryItem>> {
    return this.http.get<ApiResponse<InventoryItem>>(`${this.apiUrl}/${id}`);
  }

  createItem(data: CreateInventoryItem): Observable<ApiResponse<InventoryItem>> {
    return this.http.post<ApiResponse<InventoryItem>>(this.apiUrl, data);
  }

  updateItem(id: number, data: UpdateInventoryItem): Observable<ApiResponse<InventoryItem>> {
    return this.http.put<ApiResponse<InventoryItem>>(`${this.apiUrl}/${id}`, data);
  }

  deleteItem(id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/${id}`);
  }

  getStock(id: number): Observable<ApiResponse<number>> {
    return this.http.get<ApiResponse<number>>(`${this.apiUrl}/${id}/stock`);
  }
}
