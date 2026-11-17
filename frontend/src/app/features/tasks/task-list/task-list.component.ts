import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { TaskService } from '../../../core/services/task.service';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { TaskItem, TaskFilterParams, TaskPriority, TaskStatus } from '../../../core/models/task.model';

@Component({
  selector: 'app-task-list',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, NgbModule],
  template: `
    <div class="container-fluid py-2">
      <!-- Header -->
      <div class="d-flex justify-content-between align-items-center mb-4 pb-2 border-bottom">
        <div>
          <h2 class="fw-bold mb-0 text-dark">
            <i class="bi bi-check2-square text-primary me-2"></i>Task Manager
          </h2>
          <p class="text-muted small mb-0">Organize, prioritize, and track your daily tasks</p>
        </div>
        <button class="btn btn-primary shadow-sm" (click)="openTaskModal(taskModal)">
          <i class="bi bi-plus-lg me-1"></i> New Task
        </button>
      </div>

      <!-- Filters & Search Toolbar -->
      <div class="card shadow-sm border-0 mb-4 bg-light">
        <div class="card-body p-3">
          <div class="row g-2 align-items-center">
            <!-- Search -->
            <div class="col-md-5">
              <div class="input-group">
                <span class="input-group-text bg-white border-end-0 text-muted">
                  <i class="bi bi-search"></i>
                </span>
                <input
                  type="text"
                  class="form-control border-start-0 ps-0"
                  placeholder="Search tasks by title..."
                  [(ngModel)]="filterParams.searchTerm"
                  (keyup.enter)="applyFilters()"
                />
                <button class="btn btn-outline-secondary" type="button" (click)="applyFilters()">Search</button>
              </div>
            </div>

            <!-- Status Filter -->
            <div class="col-md-3">
              <select class="form-select" [(ngModel)]="filterParams.status" (change)="applyFilters()">
                <option value="">All Statuses</option>
                <option value="Pending">Pending</option>
                <option value="InProgress">In Progress</option>
                <option value="Completed">Completed</option>
              </select>
            </div>

            <!-- Priority Filter -->
            <div class="col-md-3">
              <select class="form-select" [(ngModel)]="filterParams.priority" (change)="applyFilters()">
                <option value="">All Priorities</option>
                <option value="Low">Low Priority</option>
                <option value="Medium">Medium Priority</option>
                <option value="High">High Priority</option>
              </select>
            </div>

            <!-- Reset Button -->
            <div class="col-md-1 text-end">
              <button class="btn btn-outline-secondary w-100" (click)="resetFilters()" title="Reset Filters">
                <i class="bi bi-arrow-counterclockwise"></i>
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Task Table Card -->
      <div class="card shadow-sm border-0">
        <div class="card-body p-0">
          <div *ngIf="isLoading()" class="text-center py-5">
            <div class="spinner-border text-primary" role="status">
              <span class="visually-hidden">Loading...</span>
            </div>
            <p class="text-muted small mt-2">Loading tasks...</p>
          </div>

          <div *ngIf="!isLoading() && tasks().length === 0" class="text-center py-5">
            <i class="bi bi-inbox text-muted fs-1 d-block mb-2"></i>
            <h5 class="text-muted">No tasks found</h5>
            <p class="text-muted small">Create a new task or adjust your search filters.</p>
          </div>

          <div class="table-responsive" *ngIf="!isLoading() && tasks().length > 0">
            <table class="table table-hover align-middle mb-0">
              <thead class="table-light text-uppercase small text-muted">
                <tr>
                  <th scope="col" class="ps-4">Title & Description</th>
                  <th scope="col">Status</th>
                  <th scope="col">Priority</th>
                  <th scope="col">Due Date</th>
                  <th scope="col" *ngIf="authService.isAdmin()">Owner</th>
                  <th scope="col" class="text-end pe-4">Actions</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let task of tasks()">
                  <td class="ps-4">
                    <div class="fw-bold text-dark">{{ task.title }}</div>
                    <div class="text-muted small text-truncate" style="max-width: 380px;">
                      {{ task.description || 'No description provided.' }}
                    </div>
                  </td>
                  <td>
                    <span class="badge rounded-pill" [ngClass]="{
                      'bg-warning text-dark': task.status === 'Pending',
                      'bg-info text-dark': task.status === 'InProgress',
                      'bg-success text-white': task.status === 'Completed'
                    }">
                      {{ task.status }}
                    </span>
                  </td>
                  <td>
                    <span class="badge" [ngClass]="{
                      'bg-secondary': task.priority === 'Low',
                      'bg-primary': task.priority === 'Medium',
                      'bg-danger': task.priority === 'High'
                    }">
                      {{ task.priority }}
                    </span>
                  </td>
                  <td class="small">
                    <span *ngIf="task.dueDate; else noDueDate">
                      <i class="bi bi-calendar3 me-1 text-muted"></i>
                      {{ task.dueDate | date:'mediumDate' }}
                    </span>
                    <ng-template #noDueDate><span class="text-muted italic">None</span></ng-template>
                  </td>
                  <td *ngIf="authService.isAdmin()">
                    <span class="badge bg-light text-dark border">
                      <i class="bi bi-person me-1"></i>{{ task.ownerName || task.ownerUserId }}
                    </span>
                  </td>
                  <td class="text-end pe-4">
                    <button class="btn btn-sm btn-outline-primary me-2" (click)="editTask(task, taskModal)">
                      <i class="bi bi-pencil"></i>
                    </button>
                    <button class="btn btn-sm btn-outline-danger" (click)="deleteTask(task)">
                      <i class="bi bi-trash"></i>
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <!-- Pagination Footer -->
        <div class="card-footer bg-white border-top d-flex justify-content-between align-items-center py-3 px-4"
             *ngIf="totalCount() > 0">
          <span class="text-muted small">
            Showing Page <strong>{{ filterParams.pageNumber }}</strong> of <strong>{{ totalPages() }}</strong>
            ({{ totalCount() }} total tasks)
          </span>
          <div class="btn-group">
            <button
              class="btn btn-outline-secondary btn-sm"
              [disabled]="filterParams.pageNumber <= 1"
              (click)="changePage(filterParams.pageNumber - 1)"
            >
              <i class="bi bi-chevron-left me-1"></i> Prev
            </button>
            <button
              class="btn btn-outline-secondary btn-sm"
              [disabled]="filterParams.pageNumber >= totalPages()"
              (click)="changePage(filterParams.pageNumber + 1)"
            >
              Next <i class="bi bi-chevron-right ms-1"></i>
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Create / Edit Modal -->
    <ng-template #taskModal let-modal>
      <div class="modal-header">
        <h5 class="modal-title fw-bold">
          <i class="bi" [ngClass]="isEditing() ? 'bi-pencil-square text-primary' : 'bi-plus-circle text-success'"></i>
          {{ isEditing() ? 'Edit Task' : 'Create New Task' }}
        </h5>
        <button type="button" class="btn-close" (click)="modal.dismiss()"></button>
      </div>
      <div class="modal-body">
        <form [formGroup]="taskForm">
          <div class="mb-3">
            <label class="form-label small fw-semibold">Task Title <span class="text-danger">*</span></label>
            <input
              type="text"
              class="form-control"
              formControlName="title"
              placeholder="e.g. Audit warehouse sector A"
              [class.is-invalid]="taskForm.get('title')?.touched && taskForm.get('title')?.invalid"
            />
            <div class="invalid-feedback small">Title is required (max 200 characters).</div>
          </div>

          <div class="mb-3">
            <label class="form-label small fw-semibold">Description</label>
            <textarea
              class="form-control"
              rows="3"
              formControlName="description"
              placeholder="Optional details or instructions..."
            ></textarea>
          </div>

          <div class="row g-2 mb-3">
            <div class="col-md-6">
              <label class="form-label small fw-semibold">Status</label>
              <select class="form-select" formControlName="status">
                <option value="Pending">Pending</option>
                <option value="InProgress">In Progress</option>
                <option value="Completed">Completed</option>
              </select>
            </div>
            <div class="col-md-6">
              <label class="form-label small fw-semibold">Priority</label>
              <select class="form-select" formControlName="priority">
                <option value="Low">Low</option>
                <option value="Medium">Medium</option>
                <option value="High">High</option>
              </select>
            </div>
          </div>

          <div class="mb-3">
            <label class="form-label small fw-semibold">Due Date</label>
            <input type="date" class="form-control" formControlName="dueDate" />
          </div>
        </form>
      </div>
      <div class="modal-footer">
        <button type="button" class="btn btn-light" (click)="modal.dismiss()">Cancel</button>
        <button
          type="button"
          class="btn btn-primary"
          [disabled]="taskForm.invalid || isSaving()"
          (click)="saveTask(modal)"
        >
          <span *ngIf="isSaving()" class="spinner-border spinner-border-sm me-1"></span>
          {{ isSaving() ? 'Saving...' : (isEditing() ? 'Save Changes' : 'Create Task') }}
        </button>
      </div>
    </ng-template>
  `
})
export class TaskListComponent implements OnInit {
  private readonly taskService = inject(TaskService);
  readonly authService = inject(AuthService);
  private readonly toastService = inject(ToastService);
  private readonly modalService = inject(NgbModal);
  private readonly fb = inject(FormBuilder);

  readonly tasks = signal<TaskItem[]>([]);
  readonly isLoading = signal(false);
  readonly isSaving = signal(false);
  readonly totalCount = signal(0);
  readonly totalPages = signal(1);
  readonly isEditing = signal(false);
  private currentTaskId: number | null = null;

  filterParams: TaskFilterParams = {
    pageNumber: 1,
    pageSize: 10,
    searchTerm: '',
    status: '',
    priority: ''
  };

  taskForm: FormGroup = this.fb.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    description: [''],
    status: ['Pending', Validators.required],
    priority: ['Medium', Validators.required],
    dueDate: ['']
  });

  ngOnInit(): void {
    this.loadTasks();
  }

  loadTasks(): void {
    this.isLoading.set(true);
    this.taskService.getTasks(this.filterParams).subscribe({
      next: (response) => {
        this.isLoading.set(false);
        if (response.success && response.data) {
          this.tasks.set(response.data.items);
          this.totalCount.set(response.data.totalCount);
          this.totalPages.set(response.data.totalPages || 1);
        }
      },
      error: () => {
        this.isLoading.set(false);
        this.toastService.showError('Failed to load tasks from server.');
      }
    });
  }

  applyFilters(): void {
    this.filterParams.pageNumber = 1;
    this.loadTasks();
  }

  resetFilters(): void {
    this.filterParams = {
      pageNumber: 1,
      pageSize: 10,
      searchTerm: '',
      status: '',
      priority: ''
    };
    this.loadTasks();
  }

  changePage(newPage: number): void {
    this.filterParams.pageNumber = newPage;
    this.loadTasks();
  }

  openTaskModal(content: any): void {
    this.isEditing.set(false);
    this.currentTaskId = null;
    this.taskForm.reset({
      title: '',
      description: '',
      status: 'Pending',
      priority: 'Medium',
      dueDate: ''
    });
    this.modalService.open(content, { backdrop: 'static', centered: true });
  }

  editTask(task: TaskItem, content: any): void {
    this.isEditing.set(true);
    this.currentTaskId = task.id;

    const formattedDate = task.dueDate ? task.dueDate.split('T')[0] : '';
    this.taskForm.patchValue({
      title: task.title,
      description: task.description || '',
      status: task.status,
      priority: task.priority,
      dueDate: formattedDate
    });

    this.modalService.open(content, { backdrop: 'static', centered: true });
  }

  saveTask(modal: any): void {
    if (this.taskForm.invalid) {
      this.taskForm.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);
    const formVal = this.taskForm.value;
    const payload = {
      title: formVal.title,
      description: formVal.description,
      status: formVal.status,
      priority: formVal.priority,
      dueDate: formVal.dueDate ? new Date(formVal.dueDate).toISOString() : undefined
    };

    if (this.isEditing() && this.currentTaskId) {
      this.taskService.updateTask(this.currentTaskId, payload).subscribe({
        next: (res) => {
          this.isSaving.set(false);
          if (res.success) {
            this.toastService.showSuccess('Task updated successfully.');
            modal.close();
            this.loadTasks();
          } else {
            this.toastService.showError(res.message || 'Failed to update task.');
          }
        },
        error: (err) => {
          this.isSaving.set(false);
          this.toastService.showError(err.error?.message || 'Error updating task.');
        }
      });
    } else {
      this.taskService.createTask(payload).subscribe({
        next: (res) => {
          this.isSaving.set(false);
          if (res.success) {
            this.toastService.showSuccess('Task created successfully.');
            modal.close();
            this.loadTasks();
          } else {
            this.toastService.showError(res.message || 'Failed to create task.');
          }
        },
        error: (err) => {
          this.isSaving.set(false);
          this.toastService.showError(err.error?.message || 'Error creating task.');
        }
      });
    }
  }

  deleteTask(task: TaskItem): void {
    if (confirm(`Are you sure you want to delete task "${task.title}"?`)) {
      this.taskService.deleteTask(task.id).subscribe({
        next: (res) => {
          if (res.success) {
            this.toastService.showSuccess('Task deleted successfully.');
            this.loadTasks();
          } else {
            this.toastService.showError(res.message || 'Failed to delete task.');
          }
        },
        error: () => {
          this.toastService.showError('Error deleting task.');
        }
      });
    }
  }
}
