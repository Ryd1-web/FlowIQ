import { Component, Inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Income } from '../../core/models/income.model';

export interface IncomeFormDialogData {
  title: string;
  record?: Income | null;
}

@Component({
  selector: 'app-income-form-dialog',
  templateUrl: './income-form-dialog.component.html',
  styleUrls: ['./management.component.scss']
})
export class IncomeFormDialogComponent {
  form: ReturnType<FormBuilder['group']>;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<IncomeFormDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: IncomeFormDialogData
  ) {
    this.form = this.fb.group({
      amount: [0, [Validators.required, Validators.min(1)]],
      source: ['', [Validators.required, Validators.maxLength(100)]],
      date: ['', Validators.required],
      notes: ['']
    });

    if (data.record) {
      this.form.patchValue({
        amount: data.record.amount,
        source: data.record.category,
        date: this.toDateValue(data.record.date),
        notes: data.record.description ?? ''
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
      type: 'Income',
      category: value.source ?? '',
      description: value.notes ?? ''
    } as Income);
  }

  close(): void {
    this.dialogRef.close();
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
