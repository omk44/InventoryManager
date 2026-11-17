export interface InventoryItem {
  id: number;
  sku: string;
  name: string;
  description?: string;
  categoryId: number;
  categoryName?: string;
  reorderLevel: number;
  currentStock: number; // Dynamically computed server-side
  isLowStock: boolean;
  ownerUserId: string;
  ownerName?: string;
  createdAt: string;
  createdBy?: string;
  updatedAt?: string;
  updatedBy?: string;
}

export interface CreateInventoryItem {
  sku: string;
  name: string;
  description?: string;
  categoryId: number;
  reorderLevel: number;
  initialStock: number;
}

export interface UpdateInventoryItem {
  sku: string;
  name: string;
  description?: string;
  categoryId: number;
  reorderLevel: number;
}

export interface InventoryFilterParams {
  pageNumber: number;
  pageSize: number;
  searchTerm?: string;
  categoryId?: number | '';
  lowStockOnly?: boolean;
}