export enum StockMovementType {
  IN = 0,
  OUT = 1,
  ADJUST = 2
}

export interface StockMovement {
  id: number;
  inventoryItemId: number;
  itemName: string;
  sku: string;
  type: StockMovementType | string;
  quantity: number;
  note?: string;
  createdAt: string;
  createdBy?: string;
}

export interface CreateStockMovement {
  inventoryItemId: number;
  type: StockMovementType;
  quantity: number;
  note?: string;
}