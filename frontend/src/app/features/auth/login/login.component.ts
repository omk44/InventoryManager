import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <div class="row justify-content-center mt-5">
      <div class="col-md-5 col-lg-4">
        <div class="card shadow-sm border-0">
          <div class="card-body p-4">
            <div class="text-center mb-4">
              <i class="bi bi-box-seam text-primary fs-1"></i>
              <h3 class="fw-bold mt-2">Sign In</h3>
              <p class="text-muted small">Access your inventory and tasks</p>
            </div>

            <div *ngIf="errorMessage()" class="alert alert-danger py-2 small" role="alert">
              {{ errorMessage() }}
            </div>

            <form [formGroup]="loginForm" (ngSubmit)="onSubmit()">
              <div class="mb-3">
                <label for="email" class="form-label small fw-semibold">Email Address</label>
                <input
                  type="email"
                  id="email"
                  class="form-control"
                  [class.is-invalid]="f['email'].touched && f['email'].invalid"
                  formControlName="email"
                  placeholder="name@example.com"
                />
                <div class="invalid-feedback small" *ngIf="f['email'].errors?.['required']">
                  Email is required.
                </div>
                <div class="invalid-feedback small" *ngIf="f['email'].errors?.['email']">
                  Enter a valid email address.
                </div>
              </div>

              <div class="mb-3">
                <label for="password" class="form-label small fw-semibold">Password</label>
                <input
                  type="password"
                  id="password"
                  class="form-control"
                  [class.is-invalid]="f['password'].touched && f['password'].invalid"
                  formControlName="password"
                  placeholder="Enter your password"
                />
                <div class="invalid-feedback small" *ngIf="f['password'].errors?.['required']">
                  Password is required.
                </div>
              </div>

              <button
                type="submit"
                class="btn btn-primary w-100 py-2 fw-semibold"
                [disabled]="loginForm.invalid || isLoading()"
              >
                <span *ngIf="isLoading()" class="spinner-border spinner-border-sm me-1"></span>
                {{ isLoading() ? 'Signing in...' : 'Sign In' }}
              </button>
            </form>

            <div class="text-center mt-4 pt-2 border-top">
              <span class="text-muted small">Don't have an account? </span>
              <a routerLink="/register" class="small fw-semibold text-decoration-none">Sign Up</a>
            </div>

            <!-- Demo Credentials Helper -->
            <div class="mt-4 p-2 bg-light rounded text-muted small">
              <div class="fw-semibold text-dark">Demo Accounts:</div>
              <div>Admin: <code>admin&#64;inventory.com</code> / <code>Admin&#64;Passw0rd2024!</code></div>
              <div>User: <code>user&#64;inventory.com</code> / <code>User&#64;Passw0rd2024!</code></div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly loginForm: FormGroup = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]]
  });

  get f() {
    return this.loginForm.controls;
  }

  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.authService.login(this.loginForm.value).subscribe({
      next: (response) => {
        this.isLoading.set(false);
        if (response.success) {
          this.router.navigate(['/inventory']);
        } else {
          this.errorMessage.set(response.message || 'Login failed.');
        }
      },
      error: (err) => {
        this.isLoading.set(false);
        const msg = err.error?.message || 'Invalid credentials or server unavailable.';
        this.errorMessage.set(msg);
      }
    });
  }
}