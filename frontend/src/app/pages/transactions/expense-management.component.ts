import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ExpenseService } from '../../core/services/expense.service';
import { BusinessService } from '../../core/services/business.service';
import { Expense } from '../../core/models/expense.model';
import { ExpenseFormDialogComponent } from './expense-form-dialog.component';
import { ConfirmDialogComponent } from './confirm-dialog.component';

interface ExpenseCategoryOption {
  label: string;
  value: number;
}

@Component({
  selector: 'app-expense-management',
  templateUrl: './expense-management.component.html',
  styleUrls: ['./management.component.scss']
})
export class ExpenseManagementComponent implements OnInit {
  displayedColumns: string[] = ['date', 'category', 'amount', 'description', 'action'];
  records: Expense[] = [];
  loading = true;
  error = '';
  businessId = '';

  categories: ExpenseCategoryOption[] = [
    { label: 'Rent', value: 1 },
    { label: 'Salary', value: 2 },
    { label: 'Supplies', value: 3 },
    { label: 'Transport', value: 4 },
    { label: 'Food', value: 5 },
    { label: 'Utilities', value: 6 },
    { label: 'Marketing', value: 7 },
    { label: 'Maintenance', value: 8 },
    { label: 'Tax', value: 9 },
    { label: 'Loan', value: 10 },
    { label: 'Inventory', value: 11 },
    { label: 'Equipment', value: 12 },
    { label: 'Other', value: 13 }
  ];

  constructor(
    private expenseService: ExpenseService,
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
    this.expenseService.getAll(this.businessId).subscribe({
      next: rows => {
        this.records = rows.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime());
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load expense records.';
        this.loading = false;
      }
    });
  }

  openAddDialog(): void {
    const dialogRef = this.dialog.open(ExpenseFormDialogComponent, {
      width: '520px',
      data: { title: 'Add Expense', categories: this.categories }
    });
    dialogRef.afterClosed().subscribe((payload?: Expense) => {
      if (!payload) {
        return;
      }
      this.expenseService.add(this.businessId, payload).subscribe({
        next: () => {
          this.showAlert('Expense added successfully.', 'success');
          this.loadRecords();
        },
        error: () => {
          this.showAlert('Failed to add expense.', 'error');
        }
      });
    });
  }

  openEditDialog(row: Expense): void {
    const dialogRef = this.dialog.open(ExpenseFormDialogComponent, {
      width: '520px',
      data: { title: 'Edit Expense', record: row, categories: this.categories }
    });
    dialogRef.afterClosed().subscribe((payload?: Expense) => {
      if (!payload || !row.id) {
        return;
      }
      this.expenseService.update(this.businessId, row.id, payload).subscribe({
        next: () => {
          this.showAlert('Expense updated successfully.', 'success');
          this.loadRecords();
        },
        error: () => {
          this.showAlert('Failed to update expense.', 'error');
        }
      });
    });
  }

  openDeleteDialog(row: Expense): void {
    if (!row.id) {
      return;
    }

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '420px',
      data: {
        title: 'Delete Expense',
        message: 'Are you sure you want to delete this expense record?',
        yesText: 'Yes',
        noText: 'No'
      }
    });

    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if (!confirmed) {
        return;
      }
      this.expenseService.delete(this.businessId, row.id!).subscribe({
        next: () => {
          this.showAlert('Expense deleted successfully.', 'success');
          this.loadRecords();
        },
        error: () => {
          this.showAlert('Failed to delete expense.', 'error');
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
