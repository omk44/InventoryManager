import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-toasts',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="toast-container position-fixed top-0 end-0 p-3" style="z-index: 1200;">
      <div
        *ngFor="let toast of toastService.toasts()"
        class="toast show align-items-center text-white border-0 mb-2 shadow"
        [ngClass]="'bg-' + toast.type"
        role="alert"
      >
        <div class="d-flex">
          <div class="toast-body fw-semibold">
            <i class="bi me-2" [ngClass]="{
              'bi-check-circle-fill': toast.type === 'success',
              'bi-exclamation-triangle-fill': toast.type === 'danger',
              'bi-info-circle-fill': toast.type === 'info',
              'bi-exclamation-circle-fill': toast.type === 'warning'
            }"></i>
            {{ toast.message }}
          </div>
          <button
            type="button"
            class="btn-close btn-close-white me-2 m-auto"
            (click)="toastService.remove(toast)"
            aria-label="Close"
          ></button>
        </div>
      </div>
    </div>
  `
})
export class ToastsContainerComponent {
  readonly toastService = inject(ToastService);
}