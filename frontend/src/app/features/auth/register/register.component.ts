import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <div class="row justify-content-center mt-5">
      <div class="col-md-6 col-lg-5">
        <div class="card shadow-sm border-0">
          <div class="card-body p-4">
            <div class="text-center mb-4">
              <i class="bi bi-person-plus text-primary fs-1"></i>
              <h3 class="fw-bold mt-2">Create Account</h3>
              <p class="text-muted small">Register to manage your tasks & inventory</p>
            </div>

            <div *ngIf="errorMessage()" class="alert alert-danger py-2 small" role="alert">
              {{ errorMessage() }}
            </div>

            <form [formGroup]="registerForm" (ngSubmit)="onSubmit()">
              <div class="mb-3">
                <label for="fullName" class="form-label small fw-semibold">Full Name</label>
                <input
                  type="text"
                  id="fullName"
                  class="form-control"
                  [class.is-invalid]="f['fullName'].touched && f['fullName'].invalid"
                  formControlName="fullName"
                  placeholder="e.g. John Doe"
                />
                <div class="invalid-feedback small" *ngIf="f['fullName'].errors?.['required']">
                  Full name is required.
                </div>
              </div>

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
                  placeholder="Min 6 characters (1 uppercase, 1 number)"
                />
                <div class="invalid-feedback small" *ngIf="f['password'].errors?.['required']">
                  Password is required.
                </div>
                <div class="invalid-feedback small" *ngIf="f['password'].errors?.['minlength']">
                  Password must be at least 6 characters.
                </div>
              </div>

              <button
                type="submit"
                class="btn btn-primary w-100 py-2 fw-semibold"
                [disabled]="registerForm.invalid || isLoading()"
              >
                <span *ngIf="isLoading()" class="spinner-border spinner-border-sm me-1"></span>
                {{ isLoading() ? 'Creating account...' : 'Create Account' }}
              </button>
            </form>

            <div class="text-center mt-4 pt-2 border-top">
              <span class="text-muted small">Already have an account? </span>
              <a routerLink="/login" class="small fw-semibold text-decoration-none">Sign In</a>
            </div>
          </div>
        </div>
      </div>
    </div>
  `
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly registerForm: FormGroup = this.fb.group({
    fullName: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  get f() {
    return this.registerForm.controls;
  }

  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.authService.register(this.registerForm.value).subscribe({
      next: (response) => {
        this.isLoading.set(false);
        if (response.success) {
          this.router.navigate(['/inventory']);
        } else {
          this.errorMessage.set(response.message || 'Registration failed.');
        }
      },
      error: (err) => {
        this.isLoading.set(false);
        const msg = err.error?.message || 'Registration failed. Check inputs or email availability.';
        this.errorMessage.set(msg);
      }
    });
  }
}