import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { CreateStockMovement, StockMovement } from '../models/stock-movement.model';

@Injectable({
  providedIn: 'root'
})
export class StockMovementService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/stockmovements`;

  getMovementsByItem(inventoryItemId: number): Observable<ApiResponse<StockMovement[]>> {
    return this.http.get<ApiResponse<StockMovement[]>>(`${this.apiUrl}/item/${inventoryItemId}`);
  }

  recordMovement(data: CreateStockMovement): Observable<ApiResponse<StockMovement>> {
    return this.http.post<ApiResponse<StockMovement>>(this.apiUrl, data);
  }
}
