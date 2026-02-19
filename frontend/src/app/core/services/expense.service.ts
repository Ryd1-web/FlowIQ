import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { Expense } from '../models/expense.model';
import { ApiResponse, unwrapApiData } from '../models/api-response.model';

@Injectable({ providedIn: 'root' })
export class ExpenseService {
  constructor(private http: HttpClient) {}

  // The businessId should be provided by the app (e.g., from user context or a service)
  private getApiUrl(businessId: string): string {
    return `${environment.apiBaseUrl}/business/${businessId}/expense`;
  }

  getAll(businessId: string): Observable<Expense[]> {
    const { from, to } = this.getDefaultDateRange();
    return this.http
      .get<ApiResponse<any[]> | any[]>(`${this.getApiUrl(businessId)}?from=${encodeURIComponent(from)}&to=${encodeURIComponent(to)}`)
      .pipe(
      map((response) => {
        const expenses = unwrapApiData<any[]>(response, []);
        return expenses
          .filter((x) => !!x)
          .map((e) => this.mapExpense(e));
      }),
      catchError(this.handleError)
    );
  }

  add(businessId: string, expense: Expense): Observable<Expense> {
    return this.http.post<ApiResponse<any> | any>(this.getApiUrl(businessId), this.toApiPayload(expense)).pipe(
      map((response) => this.mapExpense(unwrapApiData<any>(response, {}))),
      catchError(this.handleError)
    );
  }

  update(businessId: string, expenseId: string, expense: Expense): Observable<Expense> {
    return this.http.put<ApiResponse<any> | any>(`${this.getApiUrl(businessId)}/${expenseId}`, this.toApiPayload(expense)).pipe(
      map((response) => this.mapExpense(unwrapApiData<any>(response, {}))),
      catchError(this.handleError)
    );
  }

  delete(businessId: string, expenseId: string): Observable<void> {
    return this.http.delete<void>(`${this.getApiUrl(businessId)}/${expenseId}`).pipe(
      catchError(this.handleError)
    );
  }

  private handleError(error: any) {
    return throwError(() => error);
  }

  private getDefaultDateRange(): { from: string; to: string } {
    const toDate = new Date();
    toDate.setDate(toDate.getDate() + 1);
    const fromDate = new Date('2000-01-01T00:00:00.000Z');
    return {
      from: fromDate.toISOString(),
      to: toDate.toISOString()
    };
  }

  private toApiPayload(expense: Expense): any {
    return {
      amount: Number(expense.amount),
      category: this.toCategoryValue(expense.category),
      description: expense.description || expense.category || 'Expense',
      transactionDate: expense.date,
      notes: expense.description || null
    };
  }

  private mapExpense(e: any): Expense {
    return {
      id: e.id ?? e.Id,
      date: e.date ?? e.Date ?? e.transactionDate ?? e.TransactionDate,
      type: 'Expense',
      category: e.categoryName ?? e.CategoryName ?? e.category ?? e.Category ?? 'Expense',
      amount: Number(e.amount ?? e.Amount ?? 0),
      description: e.description ?? e.Description ?? e.notes ?? e.Notes
    };
  }

  private toCategoryValue(category: string): number {
    const numeric = Number(category);
    if (!Number.isNaN(numeric) && numeric > 0) {
      return numeric;
    }

    const map: Record<string, number> = {
      rent: 1,
      salary: 2,
      supplies: 3,
      transport: 4,
      food: 5,
      utilities: 6,
      marketing: 7,
      maintenance: 8,
      tax: 9,
      loan: 10,
      inventory: 11,
      equipment: 12,
      other: 13
    };
    return map[(category || '').toLowerCase()] ?? 13;
  }
}
