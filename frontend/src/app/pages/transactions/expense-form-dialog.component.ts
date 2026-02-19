import { Component, Inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Expense } from '../../core/models/expense.model';

interface ExpenseCategoryOption {
  label: string;
  value: number;
}

export interface ExpenseFormDialogData {
  title: string;
  record?: Expense | null;
  categories: ExpenseCategoryOption[];
}

@Component({
  selector: 'app-expense-form-dialog',
  templateUrl: './expense-form-dialog.component.html',
  styleUrls: ['./management.component.scss']
})
export class ExpenseFormDialogComponent {
  form: ReturnType<FormBuilder['group']>;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<ExpenseFormDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ExpenseFormDialogData
  ) {
    this.form = this.fb.group({
      amount: [0, [Validators.required, Validators.min(1)]],
      category: [13, Validators.required],
      date: ['', Validators.required],
      description: ['', [Validators.required, Validators.maxLength(200)]]
    });

    if (data.record) {
      this.form.patchValue({
        amount: data.record.amount,
        category: this.findCategoryValue(data.record.category),
        date: this.toDateValue(data.record.date),
        description: data.record.description ?? ''
      });
    }
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.getRawValue();
    const selectedDate = value.date ? new Date(value.date as any) : null;
    this.dialogRef.close({
      amount: Number(value.amount),
      date: selectedDate ? this.toLocalApiDate(selectedDate) : '',
      type: 'Expense',
      category: String(value.category ?? 13),
      description: value.description ?? ''
    } as Expense);
  }

  close(): void {
    this.dialogRef.close();
  }

  private findCategoryValue(category: string): number {
    const numeric = Number(category);
    if (!Number.isNaN(numeric) && numeric > 0) {
      return numeric;
    }
    const match = this.data.categories.find(x => x.label.toLowerCase() === (category || '').toLowerCase());
    return match?.value ?? 13;
  }

  private toDateValue(value: string): Date | null {
    if (!value) {
      return null;
    }

    const datePart = value.includes('T') ? value.split('T')[0] : value;
    const [year, month, day] = datePart.split('-').map(Number);
    if (!year || !month || !day) {
      return null;
    }
    return new Date(year, month - 1, day);
  }

  private toLocalApiDate(value: Date): string {
    const year = value.getFullYear();
    const month = String(value.getMonth() + 1).padStart(2, '0');
    const day = String(value.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}T00:00:00`;
  }
}
