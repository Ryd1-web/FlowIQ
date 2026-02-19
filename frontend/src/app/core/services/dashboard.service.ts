import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { DashboardSummary, DashboardTrends } from '../models/dashboard.model';
import { ApiResponse, unwrapApiData } from '../models/api-response.model';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private apiUrl = environment.apiBaseUrl + '/dashboard';

  constructor(private http: HttpClient) {}

  // The businessId should be provided by the app (e.g., from user context or a service)
  private getApiUrl(): string {
    return `${environment.apiBaseUrl}/dashboard`;
  }

  getSummary(businessId: string): Observable<DashboardSummary> {
    return this.http.get<ApiResponse<any> | any>(`${this.getApiUrl()}/summary/${businessId}`).pipe(
      map((response) => {
        const dto = unwrapApiData<any>(response, {});
        const todayIn = Number(dto.todayIncome ?? dto.TodayIncome ?? dto.todayIn ?? 0);
        const todayOut = Number(dto.todayExpense ?? dto.TodayExpense ?? dto.todayOut ?? 0);
        return {
          todayIn,
          todayOut,
          netBalance: Number(dto.todayNetCashflow ?? dto.TodayNetCashflow ?? dto.netBalance ?? todayIn - todayOut),
          cashflowHealth: dto.todayStatusLabel ?? dto.TodayStatusLabel ?? dto.cashflowHealth ?? 'Unknown',
          businessName: dto.businessName ?? dto.BusinessName ?? ''
        };
      }),
      catchError(this.handleError)
    );
  }

  getTrends(businessId: string, period: string = 'weekly'): Observable<DashboardTrends> {
    return this.http.get<ApiResponse<any> | any>(`${this.getApiUrl()}/trends/${businessId}?period=${period}`).pipe(
      map((response) => {
        const dto = unwrapApiData<any>(response, {});
        const trendItems = (dto.trends ?? dto.Trends ?? []) as any[];
        return {
          dates: trendItems.map((item) => this.toChartDate(item.date ?? item.Date)),
          in: trendItems.map((item) => Number(item.income ?? item.Income ?? 0)),
          out: trendItems.map((item) => Number(item.expense ?? item.Expense ?? 0)),
          net: trendItems.map((item) => Number(item.net ?? item.Net ?? 0))
        };
      }),
      catchError(this.handleError)
    );
  }

  private handleError(error: any) {
    return throwError(() => error);
  }

  private toChartDate(value: string): string {
    if (!value) {
      return value;
    }

    const datePart = value.includes('T') ? value.split('T')[0] : value;
    const parts = datePart.split('-').map((part) => Number(part));
    if (parts.length !== 3 || parts.some((part) => Number.isNaN(part))) {
      return value;
    }

    const [year, month, day] = parts;
    const utcDate = new Date(Date.UTC(year, month - 1, day));
    return utcDate.toLocaleDateString(undefined, {
      month: 'short',
      day: 'numeric',
      timeZone: 'UTC'
    });
  }
}
