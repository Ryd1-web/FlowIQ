import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { IncomeService } from '../../core/services/income.service';
import { BusinessService } from '../../core/services/business.service';
import { Income } from '../../core/models/income.model';
import { IncomeFormDialogComponent } from './income-form-dialog.component';
import { ConfirmDialogComponent } from './confirm-dialog.component';

@Component({
  selector: 'app-income-management',
  templateUrl: './income-management.component.html',
  styleUrls: ['./management.component.scss']
})
export class IncomeManagementComponent implements OnInit {
  displayedColumns: string[] = ['date', 'source', 'amount', 'notes', 'action'];
  records: Income[] = [];
  loading = true;
  error = '';
  businessId = '';

  constructor(
    private incomeService: IncomeService,
    private businessService: BusinessService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.businessService.getAllForCurrentUser().subscribe({
      next: businesses => {
        if (!businesses.length) {
          this.error = 'No business found for this user.';
          this.loading = false;
          return;
        }
        this.businessId = businesses[0].id;
        this.loadRecords();
      },
      error: () => {
        this.error = 'Failed to load business context.';
        this.loading = false;
      }
    });
  }

  loadRecords(): void {
    this.loading = true;
    this.incomeService.getAll(this.businessId).subscribe({
      next: rows => {
        this.records = rows.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime());
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load income records.';
        this.loading = false;
      }
    });
  }

  openAddDialog(): void {
    const dialogRef = this.dialog.open(IncomeFormDialogComponent, {
      width: '520px',
      data: { title: 'Add Income' }
    });
    dialogRef.afterClosed().subscribe((payload?: Income) => {
      if (!payload) {
        return;
      }
      this.incomeService.add(this.businessId, payload).subscribe({
        next: () => {
          this.showAlert('Income added successfully.', 'success');
          this.loadRecords();
        },
        error: () => {
          this.showAlert('Failed to add income.', 'error');
        }
      });
    });
  }

  openEditDialog(row: Income): void {
    const dialogRef = this.dialog.open(IncomeFormDialogComponent, {
      width: '520px',
      data: { title: 'Edit Income', record: row }
    });
    dialogRef.afterClosed().subscribe((payload?: Income) => {
      if (!payload || !row.id) {
        return;
      }
      this.incomeService.update(this.businessId, row.id, payload).subscribe({
        next: () => {
          this.showAlert('Income updated successfully.', 'success');
          this.loadRecords();
        },
        error: () => {
          this.showAlert('Failed to update income.', 'error');
        }
      });
    });
  }

  openDeleteDialog(row: Income): void {
    if (!row.id) {
      return;
    }

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '420px',
      data: {
        title: 'Delete Income',
        message: 'Are you sure you want to delete this income record?',
        yesText: 'Yes',
        noText: 'No'
      }
    });

    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if (!confirmed) {
        return;
      }
      this.incomeService.delete(this.businessId, row.id!).subscribe({
        next: () => {
          this.showAlert('Income deleted successfully.', 'success');
          this.loadRecords();
        },
        error: () => {
          this.showAlert('Failed to delete income.', 'error');
        }
      });
    });
  }

  private showAlert(message: string, type: 'success' | 'error'): void {
    this.snackBar.open(message, 'Close', {
      duration: 3000,
      panelClass: type === 'success' ? 'success-snackbar' : 'error-snackbar'
    });
  }
}
