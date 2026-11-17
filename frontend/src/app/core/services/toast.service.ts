import { Injectable, signal } from '@angular/core';

export interface ToastInfo {
  message: string;
  type: 'success' | 'danger' | 'info' | 'warning';
  delay?: number;
}

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  readonly toasts = signal<ToastInfo[]>([]);

  show(message: string, type: 'success' | 'danger' | 'info' | 'warning' = 'info', delay = 4000): void {
    const toast: ToastInfo = { message, type, delay };
    this.toasts.update(current => [...current, toast]);

    setTimeout(() => {
      this.remove(toast);
    }, delay);
  }

  showSuccess(message: string): void {
    this.show(message, 'success');
  }

  showError(message: string): void {
    this.show(message, 'danger', 6000);
  }

  remove(toast: ToastInfo): void {
    this.toasts.update(current => current.filter(t => t !== toast));
  }
}