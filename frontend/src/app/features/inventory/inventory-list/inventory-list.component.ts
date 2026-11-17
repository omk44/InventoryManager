import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { InventoryService } from '../../../core/services/inventory.service';
import { CategoryService } from '../../../core/services/category.service';
import { StockMovementService } from '../../../core/services/stock-movement.service';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { InventoryItem, InventoryFilterParams } from '../../../core/models/inventory.model';
import { Category } from '../../../core/models/category.model';
import { StockMovement, StockMovementType } from '../../../core/models/stock-movement.model';

@Component({
  selector: 'app-inventory-list',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, NgbModule],
  template: `
    <div class="container-fluid py-2">
      <!-- Header -->
      <div class="d-flex justify-content-between align-items-center mb-4 pb-2 border-bottom">
        <div>
          <h2 class="fw-bold mb-0 text-dark">
            <i class="bi bi-boxes text-primary me-2"></i>Inventory Manager
          </h2>
          <p class="text-muted small mb-0">Dynamic ledger-based stock tracking & supply monitoring</p>
        </div>
        <button class="btn btn-primary shadow-sm" (click)="openItemModal(itemModal)">
          <i class="bi bi-plus-lg me-1"></i> New Item
        </button>
      </div>

      <!-- Filters & Search Toolbar -->
      <div class="card shadow-sm border-0 mb-4 bg-light">
        <div class="card-body p-3">
          <div class="row g-2 align-items-center">
            <!-- Search by Name or SKU -->
            <div class="col-md-4">
              <div class="input-group">
                <span class="input-group-text bg-white border-end-0 text-muted">
                  <i class="bi bi-search"></i>
                </span>
                <input
                  type="text"
                  class="form-control border-start-0 ps-0"
                  placeholder="Search by Name or SKU..."
                  [(ngModel)]="filterParams.searchTerm"
                  (keyup.enter)="applyFilters()"
                />
                <button class="btn btn-outline-secondary" type="button" (click)="applyFilters()">Search</button>
              </div>
            </div>

            <!-- Category Filter -->
            <div class="col-md-3">
              <select class="form-select" [(ngModel)]="filterParams.categoryId" (change)="applyFilters()">
                <option value="">All Categories</option>
                <option *ngFor="let cat of categories()" [value]="cat.id">{{ cat.name }}</option>
              </select>
            </div>

            <!-- Low Stock Filter Toggle -->
            <div class="col-md-3">
              <div class="form-check form-switch mt-1">
                <input
                  class="form-check-input"
                  type="checkbox"
                  role="switch"
                  id="lowStockSwitch"
                  [(ngModel)]="filterParams.lowStockOnly"
                  (change)="applyFilters()"
                />
                <label class="form-check-label small fw-semibold" for="lowStockSwitch">
                  <i class="bi bi-exclamation-triangle-fill text-danger me-1"></i> Low Stock Only
                </label>
              </div>
            </div>

            <!-- Reset Button -->
            <div class="col-md-2 text-end">
              <button class="btn btn-outline-secondary w-100" (click)="resetFilters()" title="Reset Filters">
                <i class="bi bi-arrow-counterclockwise me-1"></i> Reset
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Inventory Table Card -->
      <div class="card shadow-sm border-0">
        <div class="card-body p-0">
          <div *ngIf="isLoading()" class="text-center py-5">
            <div class="spinner-border text-primary" role="status">
              <span class="visually-hidden">Loading...</span>
            </div>
            <p class="text-muted small mt-2">Loading inventory items...</p>
          </div>

          <div *ngIf="!isLoading() && items().length === 0" class="text-center py-5">
            <i class="bi bi-box text-muted fs-1 d-block mb-2"></i>
            <h5 class="text-muted">No inventory items found</h5>
            <p class="text-muted small">Create a new item or adjust your search filters.</p>
          </div>

          <div class="table-responsive" *ngIf="!isLoading() && items().length > 0">
            <table class="table table-hover align-middle mb-0">
              <thead class="table-light text-uppercase small text-muted">
                <tr>
                  <th scope="col" class="ps-4">SKU / Item</th>
                  <th scope="col">Category</th>
                  <th scope="col">Reorder Level</th>
                  <th scope="col">Computed Stock</th>
                  <th scope="col" *ngIf="authService.isAdmin()">Owner</th>
                  <th scope="col" class="text-end pe-4">Actions</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let item of items()">
                  <td class="ps-4">
                    <span class="badge bg-secondary text-uppercase font-monospace mb-1">{{ item.sku }}</span>
                    <div class="fw-bold text-dark">{{ item.name }}</div>
                    <div class="text-muted small text-truncate" style="max-width: 320px;">
                      {{ item.description || 'No description provided.' }}
                    </div>
                  </td>
                  <td>
                    <span class="badge bg-light text-dark border">
                      <i class="bi bi-tag me-1"></i>{{ item.categoryName || 'Unassigned' }}
                    </span>
                  </td>
                  <td>
                    <span class="small fw-semibold text-muted">{{ item.reorderLevel }} units</span>
                  </td>
                  <td>
                    <div class="d-flex align-items-center">
                      <span
                        class="badge rounded-pill px-3 py-2 fs-6 shadow-sm me-2"
                        [ngClass]="item.isLowStock ? 'bg-danger text-white' : 'bg-success text-white'"
                      >
                        {{ item.currentStock }}
                      </span>
                      <span *ngIf="item.isLowStock" class="badge bg-warning text-dark small" title="Stock at or below reorder level">
                        <i class="bi bi-exclamation-triangle-fill me-1"></i>Low
                      </span>
                    </div>
                  </td>
                  <td *ngIf="authService.isAdmin()">
                    <span class="badge bg-light text-dark border">
                      <i class="bi bi-person me-1"></i>{{ item.ownerName || item.ownerUserId }}
                    </span>
                  </td>
                  <td class="text-end pe-4">
                    <!-- Record Stock Movement Button -->
                    <button
                      class="btn btn-sm btn-outline-success me-1"
                      (click)="openMovementModal(item, movementModal)"
                      title="Record Stock Movement (IN/OUT/ADJUST)"
                    >
                      <i class="bi bi-arrow-left-right me-1"></i> Stock
                    </button>
                    <!-- History Button -->
                    <button
                      class="btn btn-sm btn-outline-info me-1"
                      (click)="openHistoryModal(item, historyModal)"
                      title="View Ledger Transaction History"
                    >
                      <i class="bi bi-clock-history"></i>
                    </button>
                    <!-- Edit Button -->
                    <button class="btn btn-sm btn-outline-primary me-1" (click)="editItem(item, itemModal)">
                      <i class="bi bi-pencil"></i>
                    </button>
                    <!-- Delete Button -->
                    <button class="btn btn-sm btn-outline-danger" (click)="deleteItem(item)">
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
            ({{ totalCount() }} total items)
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

    <!-- 1. Create / Edit Item Modal -->
    <ng-template #itemModal let-modal>
      <div class="modal-header">
        <h5 class="modal-title fw-bold">
          <i class="bi" [ngClass]="isEditing() ? 'bi-pencil-square text-primary' : 'bi-plus-circle text-success'"></i>
          {{ isEditing() ? 'Edit Inventory Item' : 'New Inventory Item' }}
        </h5>
        <button type="button" class="btn-close" (click)="modal.dismiss()"></button>
      </div>
      <div class="modal-body">
        <form [formGroup]="itemForm">
          <div class="row g-2 mb-3">
            <div class="col-md-6">
              <label class="form-label small fw-semibold">SKU <span class="text-danger">*</span></label>
              <input
                type="text"
                class="form-control"
                formControlName="sku"
                placeholder="e.g. LAPTOP-XPS-15"
                [class.is-invalid]="itemForm.get('sku')?.touched && itemForm.get('sku')?.invalid"
              />
              <div class="invalid-feedback small">SKU is required (unique per user).</div>
            </div>
            <div class="col-md-6">
              <label class="form-label small fw-semibold">Category <span class="text-danger">*</span></label>
              <select class="form-select" formControlName="categoryId">
                <option *ngFor="let cat of categories()" [value]="cat.id">{{ cat.name }}</option>
              </select>
            </div>
          </div>

          <div class="mb-3">
            <label class="form-label small fw-semibold">Item Name <span class="text-danger">*</span></label>
            <input
              type="text"
              class="form-control"
              formControlName="name"
              placeholder="e.g. Dell XPS 15 32GB RAM"
              [class.is-invalid]="itemForm.get('name')?.touched && itemForm.get('name')?.invalid"
            />
            <div class="invalid-feedback small">Item name is required.</div>
          </div>

          <div class="mb-3">
            <label class="form-label small fw-semibold">Description</label>
            <textarea
              class="form-control"
              rows="2"
              formControlName="description"
              placeholder="Optional item specifications..."
            ></textarea>
          </div>

          <div class="row g-2 mb-3">
            <div class="col-md-6">
              <label class="form-label small fw-semibold">Reorder Alert Level</label>
              <input type="number" class="form-control" formControlName="reorderLevel" min="0" />
              <div class="form-text small">Trigger warning when stock drops to or below this.</div>
            </div>
            <div class="col-md-6" *ngIf="!isEditing()">
              <label class="form-label small fw-semibold">Initial Stock Intake</label>
              <input type="number" class="form-control" formControlName="initialStock" min="0" />
              <div class="form-text small">Initial IN ledger movement quantity.</div>
            </div>
          </div>
        </form>
      </div>
      <div class="modal-footer">
        <button type="button" class="btn btn-light" (click)="modal.dismiss()">Cancel</button>
        <button
          type="button"
          class="btn btn-primary"
          [disabled]="itemForm.invalid || isSaving()"
          (click)="saveItem(modal)"
        >
          <span *ngIf="isSaving()" class="spinner-border spinner-border-sm me-1"></span>
          {{ isSaving() ? 'Saving...' : (isEditing() ? 'Save Changes' : 'Create Item') }}
        </button>
      </div>
    </ng-template>

    <!-- 2. Stock Movement Modal -->
    <ng-template #movementModal let-modal>
      <div class="modal-header bg-light">
        <h5 class="modal-title fw-bold">
          <i class="bi bi-arrow-left-right text-success me-2"></i>Record Stock Movement
        </h5>
        <button type="button" class="btn-close" (click)="modal.dismiss()"></button>
      </div>
      <div class="modal-body">
        <div class="p-3 mb-3 bg-white border rounded">
          <div class="small text-muted text-uppercase">Target Item</div>
          <h5 class="fw-bold mb-1">{{ selectedItem?.name }}</h5>
          <div class="d-flex justify-content-between align-items-center mt-2 pt-2 border-top">
            <span class="small text-muted font-monospace">SKU: {{ selectedItem?.sku }}</span>
            <span class="badge bg-primary fs-6">Available Stock: {{ selectedItem?.currentStock }}</span>
          </div>
        </div>

        <form [formGroup]="movementForm">
          <!-- Movement Type -->
          <div class="mb-3">
            <label class="form-label small fw-semibold d-block">Movement Type</label>
            <div class="btn-group w-100" role="group">
              <input
                type="radio"
                class="btn-check"
                id="typeIn"
                [value]="0"
                formControlName="type"
              />
              <label class="btn btn-outline-success" for="typeIn">
                <i class="bi bi-plus-circle me-1"></i> IN (Add)
              </label>

              <input
                type="radio"
                class="btn-check"
                id="typeOut"
                [value]="1"
                formControlName="type"
              />
              <label class="btn btn-outline-danger" for="typeOut">
                <i class="bi bi-dash-circle me-1"></i> OUT (Deduct)
              </label>

              <input
                type="radio"
                class="btn-check"
                id="typeAdjust"
                [value]="2"
                formControlName="type"
              />
              <label class="btn btn-outline-warning" for="typeAdjust">
                <i class="bi bi-sliders me-1"></i> ADJUST (Baseline)
              </label>
            </div>
          </div>

          <!-- Quantity -->
          <div class="mb-3">
            <label class="form-label small fw-semibold">
              Quantity <span class="text-danger">*</span>
            </label>
            <input
              type="number"
              class="form-control form-control-lg"
              formControlName="quantity"
              min="1"
              placeholder="Enter quantity"
              [class.is-invalid]="isInsufficientStock() || (movementForm.get('quantity')?.touched && movementForm.get('quantity')?.invalid)"
            />
            <div class="invalid-feedback small" *ngIf="isInsufficientStock()">
              Cannot withdraw more than available stock ({{ selectedItem?.currentStock }} available).
            </div>
          </div>

          <!-- Note -->
          <div class="mb-3">
            <label class="form-label small fw-semibold">Reason / Note</label>
            <input
              type="text"
              class="form-control"
              formControlName="note"
              placeholder="e.g. Sold to invoice #4059, Restock supplier intake, Shelf audit count"
            />
          </div>
        </form>
      </div>
      <div class="modal-footer">
        <button type="button" class="btn btn-light" (click)="modal.dismiss()">Cancel</button>
        <button
          type="button"
          class="btn btn-success"
          [disabled]="movementForm.invalid || isInsufficientStock() || isSaving()"
          (click)="saveMovement(modal)"
        >
          <span *ngIf="isSaving()" class="spinner-border spinner-border-sm me-1"></span>
          {{ isSaving() ? 'Recording...' : 'Commit Movement' }}
        </button>
      </div>
    </ng-template>

    <!-- 3. Transaction History Modal -->
    <ng-template #historyModal let-modal>
      <div class="modal-header bg-light">
        <h5 class="modal-title fw-bold">
          <i class="bi bi-clock-history text-info me-2"></i>Ledger Audit Log
        </h5>
        <button type="button" class="btn-close" (click)="modal.dismiss()"></button>
      </div>
      <div class="modal-body p-0">
        <div class="p-3 border-bottom bg-white">
          <h6 class="fw-bold mb-0">{{ selectedItem?.name }}</h6>
          <span class="text-muted small font-monospace">SKU: {{ selectedItem?.sku }}</span>
        </div>

        <div *ngIf="isLoadingMovements()" class="text-center py-4">
          <div class="spinner-border spinner-border-sm text-primary" role="status"></div>
          <span class="small ms-2 text-muted">Fetching audit history...</span>
        </div>

        <div *ngIf="!isLoadingMovements() && itemMovements().length === 0" class="text-center py-4 text-muted small">
          No transactions recorded yet for this item.
        </div>

        <div class="table-responsive" *ngIf="!isLoadingMovements() && itemMovements().length > 0">
          <table class="table table-sm table-striped align-middle mb-0">
            <thead class="table-light small">
              <tr>
                <th class="ps-3">Date</th>
                <th>Type</th>
                <th>Quantity</th>
                <th>Reason / Note</th>
                <th>Recorded By</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let m of itemMovements()">
                <td class="ps-3 small text-muted">{{ m.createdAt | date:'short' }}</td>
                <td>
                  <span class="badge" [ngClass]="{
                    'bg-success': m.type === 'IN' || m.type === 0,
                    'bg-danger': m.type === 'OUT' || m.type === 1,
                    'bg-warning text-dark': m.type === 'ADJUST' || m.type === 2
                  }">
                    {{ m.type }}
                  </span>
                </td>
                <td class="fw-bold">{{ m.quantity }}</td>
                <td class="small text-muted">{{ m.note || '-' }}</td>
                <td class="small text-muted">{{ m.createdBy || 'System' }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
      <div class="modal-footer">
        <button type="button" class="btn btn-secondary btn-sm" (click)="modal.dismiss()">Close</button>
      </div>
    </ng-template>
  `
})
export class InventoryListComponent implements OnInit {
  private readonly inventoryService = inject(InventoryService);
  private readonly categoryService = inject(CategoryService);
  private readonly movementService = inject(StockMovementService);
  readonly authService = inject(AuthService);
  private readonly toastService = inject(ToastService);
  private readonly modalService = inject(NgbModal);
  private readonly fb = inject(FormBuilder);

  readonly items = signal<InventoryItem[]>([]);
  readonly categories = signal<Category[]>([]);
  readonly itemMovements = signal<StockMovement[]>([]);

  readonly isLoading = signal(false);
  readonly isSaving = signal(false);
  readonly isLoadingMovements = signal(false);
  readonly totalCount = signal(0);
  readonly totalPages = signal(1);
  readonly isEditing = signal(false);

  selectedItem: InventoryItem | null = null;
  private currentItemId: number | null = null;

  filterParams: InventoryFilterParams = {
    pageNumber: 1,
    pageSize: 10,
    searchTerm: '',
    categoryId: '',
    lowStockOnly: false
  };

  itemForm: FormGroup = this.fb.group({
    sku: ['', [Validators.required, Validators.maxLength(100)]],
    name: ['', [Validators.required, Validators.maxLength(200)]],
    description: [''],
    categoryId: [1, Validators.required],
    reorderLevel: [0, [Validators.required, Validators.min(0)]],
    initialStock: [0, [Validators.min(0)]]
  });

  movementForm: FormGroup = this.fb.group({
    type: [0, Validators.required], // 0: IN, 1: OUT, 2: ADJUST
    quantity: [1, [Validators.required, Validators.min(1)]],
    note: ['']
  });

  ngOnInit(): void {
    this.loadCategories();
    this.loadItems();
  }

  loadCategories(): void {
    this.categoryService.getCategories().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.categories.set(res.data);
          if (res.data.length > 0 && !this.itemForm.get('categoryId')?.value) {
            this.itemForm.patchValue({ categoryId: res.data[0].id });
          }
        }
      }
    });
  }

  loadItems(): void {
    this.isLoading.set(true);
    this.inventoryService.getItems(this.filterParams).subscribe({
      next: (response) => {
        this.isLoading.set(false);
        if (response.success && response.data) {
          this.items.set(response.data.items);
          this.totalCount.set(response.data.totalCount);
          this.totalPages.set(response.data.totalPages || 1);
        }
      },
      error: () => {
        this.isLoading.set(false);
        this.toastService.showError('Failed to load inventory items.');
      }
    });
  }

  applyFilters(): void {
    this.filterParams.pageNumber = 1;
    this.loadItems();
  }

  resetFilters(): void {
    this.filterParams = {
      pageNumber: 1,
      pageSize: 10,
      searchTerm: '',
      categoryId: '',
      lowStockOnly: false
    };
    this.loadItems();
  }

  changePage(newPage: number): void {
    this.filterParams.pageNumber = newPage;
    this.loadItems();
  }

  openItemModal(content: any): void {
    this.isEditing.set(false);
    this.currentItemId = null;
    const defaultCatId = this.categories().length > 0 ? this.categories()[0].id : 1;
    this.itemForm.reset({
      sku: '',
      name: '',
      description: '',
      categoryId: defaultCatId,
      reorderLevel: 5,
      initialStock: 10
    });
    this.modalService.open(content, { backdrop: 'static', centered: true });
  }

  editItem(item: InventoryItem, content: any): void {
    this.isEditing.set(true);
    this.currentItemId = item.id;
    this.itemForm.patchValue({
      sku: item.sku,
      name: item.name,
      description: item.description || '',
      categoryId: item.categoryId,
      reorderLevel: item.reorderLevel,
      initialStock: 0
    });
    this.modalService.open(content, { backdrop: 'static', centered: true });
  }

  saveItem(modal: any): void {
    if (this.itemForm.invalid) {
      this.itemForm.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);
    const formVal = this.itemForm.value;

    if (this.isEditing() && this.currentItemId) {
      this.inventoryService.updateItem(this.currentItemId, {
        sku: formVal.sku,
        name: formVal.name,
        description: formVal.description,
        categoryId: Number(formVal.categoryId),
        reorderLevel: Number(formVal.reorderLevel)
      }).subscribe({
        next: (res) => {
          this.isSaving.set(false);
          if (res.success) {
            this.toastService.showSuccess('Inventory item updated successfully.');
            modal.close();
            this.loadItems();
          } else {
            this.toastService.showError(res.message || 'Failed to update item.');
          }
        },
        error: (err) => {
          this.isSaving.set(false);
          this.toastService.showError(err.error?.message || 'Error updating item.');
        }
      });
    } else {
      this.inventoryService.createItem({
        sku: formVal.sku,
        name: formVal.name,
        description: formVal.description,
        categoryId: Number(formVal.categoryId),
        reorderLevel: Number(formVal.reorderLevel),
        initialStock: Number(formVal.initialStock || 0)
      }).subscribe({
        next: (res) => {
          this.isSaving.set(false);
          if (res.success) {
            this.toastService.showSuccess('Inventory item created successfully.');
            modal.close();
            this.loadItems();
          } else {
            this.toastService.showError(res.message || 'Failed to create item.');
          }
        },
        error: (err) => {
          this.isSaving.set(false);
          this.toastService.showError(err.error?.message || 'Error creating item.');
        }
      });
    }
  }

  deleteItem(item: InventoryItem): void {
    if (confirm(`Are you sure you want to delete "${item.name}" and all its transaction history?`)) {
      this.inventoryService.deleteItem(item.id).subscribe({
        next: (res) => {
          if (res.success) {
            this.toastService.showSuccess('Item deleted successfully.');
            this.loadItems();
          } else {
            this.toastService.showError(res.message || 'Failed to delete item.');
          }
        },
        error: () => {
          this.toastService.showError('Error deleting item.');
        }
      });
    }
  }

  // Stock Movement Modal Logic
  openMovementModal(item: InventoryItem, content: any): void {
    this.selectedItem = item;
    this.movementForm.reset({
      type: 0, // IN
      quantity: 1,
      note: ''
    });
    this.modalService.open(content, { backdrop: 'static', centered: true });
  }

  isInsufficientStock(): boolean {
    if (!this.selectedItem) return false;
    const type = this.movementForm.get('type')?.value;
    const qty = this.movementForm.get('quantity')?.value;
    // Type 1 is OUT
    return (type === 1 || type === 'OUT') && qty > this.selectedItem.currentStock;
  }

  saveMovement(modal: any): void {
    if (this.movementForm.invalid || !this.selectedItem || this.isInsufficientStock()) {
      return;
    }

    this.isSaving.set(true);
    const formVal = this.movementForm.value;

    this.movementService.recordMovement({
      inventoryItemId: this.selectedItem.id,
      type: Number(formVal.type),
      quantity: Number(formVal.quantity),
      note: formVal.note
    }).subscribe({
      next: (res) => {
        this.isSaving.set(false);
        if (res.success) {
          this.toastService.showSuccess('Stock movement recorded successfully!');
          modal.close();
          this.loadItems(); // Refreshes dynamic stock count on the table!
        } else {
          this.toastService.showError(res.message || 'Failed to record movement.');
        }
      },
      error: (err) => {
        this.isSaving.set(false);
        this.toastService.showError(err.error?.message || 'Error recording movement.');
      }
    });
  }

  // History Modal Logic
  openHistoryModal(item: InventoryItem, content: any): void {
    this.selectedItem = item;
    this.isLoadingMovements.set(true);
    this.itemMovements.set([]);

    this.modalService.open(content, { size: 'lg', centered: true });

    this.movementService.getMovementsByItem(item.id).subscribe({
      next: (res) => {
        this.isLoadingMovements.set(false);
        if (res.success && res.data) {
          this.itemMovements.set(res.data);
        }
      },
      error: () => {
        this.isLoadingMovements.set(false);
        this.toastService.showError('Failed to load transaction history.');
      }
    });
  }
}
