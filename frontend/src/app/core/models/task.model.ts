export type TaskStatus = 'Pending' | 'InProgress' | 'Completed';
export type TaskPriority = 'Low' | 'Medium' | 'High';

export interface TaskItem {
  id: number;
  title: string;
  description?: string;
  dueDate?: string;
  status: TaskStatus;
  priority: TaskPriority;
  ownerUserId: string;
  ownerName?: string;
  createdAt: string;
  createdBy?: string;
  updatedAt?: string;
  updatedBy?: string;
}

export interface CreateTaskItem {
  title: string;
  description?: string;
  dueDate?: string;
  status: TaskStatus;
  priority: TaskPriority;
}

export interface UpdateTaskItem {
  title: string;
  description?: string;
  dueDate?: string;
  status: TaskStatus;
  priority: TaskPriority;
}

export interface TaskFilterParams {
  pageNumber: number;
  pageSize: number;
  searchTerm?: string;
  status?: TaskStatus | '';
  priority?: TaskPriority | '';
}