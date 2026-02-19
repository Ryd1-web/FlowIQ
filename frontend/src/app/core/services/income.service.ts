import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { Income } from '../models/income.model';
import { ApiResponse, unwrapApiData } from '../models/api-response.model';

@Injectable({ providedIn: 'root' })
export class IncomeService {
  constructor(private http: HttpClient) {}

  // The businessId should be provided by the app (e.g., from user context or a service)
  private getApiUrl(businessId: string): string {
    return `${environment.apiBaseUrl}/business/${businessId}/income`;
  }

  getAll(businessId: string): Observable<Income[]> {
    const { from, to } = this.getDefaultDateRange();
    return this.http
      .get<ApiResponse<any[]> | any[]>(`${this.getApiUrl(businessId)}?from=${encodeURIComponent(from)}&to=${encodeURIComponent(to)}`)
      .pipe(
      map((response) => {
        const incomes = unwrapApiData<any[]>(response, []);
        return incomes
          .filter((x) => !!x)
          .map((i) => this.mapIncome(i));
      }),
      catchError(this.handleError)
    );
  }

  add(businessId: string, income: Income): Observable<Income> {
    return this.http.post<ApiResponse<any> | any>(this.getApiUrl(businessId), this.toApiPayload(income)).pipe(
      map((response) => this.mapIncome(unwrapApiData<any>(response, {}))),
      catchError(this.handleError)
    );
  }

  update(businessId: string, incomeId: string, income: Income): Observable<Income> {
    return this.http.put<ApiResponse<any> | any>(`${this.getApiUrl(businessId)}/${incomeId}`, this.toApiPayload(income)).pipe(
      map((response) => this.mapIncome(unwrapApiData<any>(response, {}))),
      catchError(this.handleError)
    );
  }

  delete(businessId: string, incomeId: string): Observable<void> {
    return this.http.delete<void>(`${this.getApiUrl(businessId)}/${incomeId}`).pipe(
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

  private toApiPayload(income: Income): any {
    return {
      amount: Number(income.amount),
      source: income.category || income.type || 'Income',
      transactionDate: income.date,
      notes: income.description || null
    };
  }

  private mapIncome(i: any): Income {
    return {
      id: i.id ?? i.Id,
      date: i.date ?? i.Date ?? i.transactionDate ?? i.TransactionDate,
      type: 'Income',
      category: i.category ?? i.Category ?? i.source ?? i.Source ?? 'Income',
      amount: Number(i.amount ?? i.Amount ?? 0),
      description: i.description ?? i.Description ?? i.notes ?? i.Notes
    };
  }
}
