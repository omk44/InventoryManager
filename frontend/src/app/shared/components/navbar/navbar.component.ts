import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  template: `
    <nav class="navbar navbar-expand-lg navbar-dark bg-dark shadow-sm">
      <div class="container-fluid px-4">
        <a class="navbar-brand fw-bold d-flex align-items-center" routerLink="/">
          <i class="bi bi-box-seam me-2 text-warning"></i>
          Inventory & Task Manager
        </a>

        <div class="collapse navbar-collapse show" id="navbarNav">
          <ul class="navbar-nav me-auto mb-2 mb-lg-0" *ngIf="authService.isAuthenticated()">
            <li class="nav-item">
              <a class="nav-link" routerLink="/inventory" routerLinkActive="active">
                <i class="bi bi-boxes me-1"></i> Inventory
              </a>
            </li>
            <li class="nav-item">
              <a class="nav-link" routerLink="/tasks" routerLinkActive="active">
                <i class="bi bi-check2-square me-1"></i> Tasks
              </a>
            </li>
          </ul>

          <div class="d-flex align-items-center ms-auto">
            <ng-container *ngIf="authService.isAuthenticated(); else guestLinks">
              <span class="navbar-text me-3 text-light small">
                <i class="bi bi-person-circle me-1"></i>
                {{ authService.currentUser()?.fullName }}
                <span class="badge bg-secondary ms-1">{{ authService.isAdmin() ? 'Admin' : 'User' }}</span>
              </span>
              <button class="btn btn-outline-light btn-sm" (click)="authService.logout()">
                <i class="bi bi-box-arrow-right me-1"></i> Sign Out
              </button>
            </ng-container>

            <ng-template #guestLinks>
              <a class="btn btn-outline-light btn-sm me-2" routerLink="/login">Sign In</a>
              <a class="btn btn-primary btn-sm" routerLink="/register">Sign Up</a>
            </ng-template>
          </div>
        </div>
      </div>
    </nav>
  `
})
export class NavbarComponent {
  readonly authService = inject(AuthService);
}