import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { TransactionsComponent } from './transactions.component';
import { IncomeManagementComponent } from './income-management.component';
import { ExpenseManagementComponent } from './expense-management.component';
import { IncomeFormDialogComponent } from './income-form-dialog.component';
import { ExpenseFormDialogComponent } from './expense-form-dialog.component';
import { ConfirmDialogComponent } from './confirm-dialog.component';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatDialogModule } from '@angular/material/dialog';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatMenuModule } from '@angular/material/menu';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';

const routes: Routes = [
  { path: '', redirectTo: 'history', pathMatch: 'full' },
  { path: 'history', component: TransactionsComponent },
  { path: 'income', component: IncomeManagementComponent },
  { path: 'expense', component: ExpenseManagementComponent }
];

@NgModule({
  declarations: [
    TransactionsComponent,
    IncomeManagementComponent,
    ExpenseManagementComponent,
    IncomeFormDialogComponent,
    ExpenseFormDialogComponent,
    ConfirmDialogComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatIconModule,
    MatButtonModule,
    MatSelectModule,
    MatDialogModule,
    MatSnackBarModule,
    MatMenuModule,
    MatDatepickerModule,
    MatNativeDateModule,
    RouterModule.forChild(routes)
  ],
  exports: [TransactionsComponent, IncomeManagementComponent, ExpenseManagementComponent]
})
export class TransactionsModule {}
