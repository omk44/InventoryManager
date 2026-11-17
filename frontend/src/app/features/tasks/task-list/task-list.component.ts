import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-task-list',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="card shadow-sm border-0 p-4">
      <div class="d-flex justify-content-between align-items-center mb-3">
        <h3 class="fw-bold m-0"><i class="bi bi-check2-square me-2 text-primary"></i>Tasks</h3>
      </div>
      <p class="text-muted">Task feature UI will be fully built in Phase 7.</p>
    </div>
  `
})
export class TaskListComponent {}
