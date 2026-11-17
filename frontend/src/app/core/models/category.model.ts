export interface Category {
  id: number;
  name: string;
  createdAt: string;
  createdBy?: string;
  itemCount: number;
}

export interface CreateCategory {
  name: string;
}

export interface UpdateCategory {
  name: string;
}