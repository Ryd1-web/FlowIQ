import { Component, OnInit, ViewChild } from '@angular/core';
import { IncomeService } from '../../core/services/income.service';
import { ExpenseService } from '../../core/services/expense.service';
import { Income } from '../../core/models/income.model';
import { Expense } from '../../core/models/expense.model';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { BusinessService, Business } from '../../core/services/business.service';
import { forkJoin } from 'rxjs';

interface TransactionRow {
  date: string;
  type: string;
  category: string;
  amount: number;
  source: 'income' | 'expense';
  id?: string;
}

@Component({
  selector: 'app-transactions',
  templateUrl: './transactions.component.html',
  styleUrls: ['./transactions.component.scss']
})
export class TransactionsComponent implements OnInit {
  displayedColumns: string[] = ['date', 'type', 'category', 'amount', 'action'];
  dataSource = new MatTableDataSource<TransactionRow>([]);
  loading = true;
  error = '';
  searchText = '';
  selectedType: 'all' | 'income' | 'expense' = 'all';

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private incomeService: IncomeService,
    private expenseService: ExpenseService,
    private businessService: BusinessService
  ) {}

  ngOnInit() {
    this.dataSource.filterPredicate = (row, filterJson) => {
      const criteria = JSON.parse(filterJson) as { searchText: string; selectedType: string };
      const text = `${row.date} ${row.type} ${row.category} ${row.amount}`.toLowerCase();
      const textMatches = text.includes(criteria.searchText);
      const typeMatches = criteria.selectedType === 'all' || row.source === criteria.selectedType;
      return textMatches && typeMatches;
    };

    this.businessService.getAllForCurrentUser().subscribe({
      next: (businesses: Business[]) => {
        if (businesses.length === 0) {
          this.error = 'No business found for this user.';
          this.loading = false;
          return;
        }
        const businessId = businesses[0].id;
        localStorage.setItem('businessId', businessId);
        this.loadTransactions(businessId);
      },
      error: () => {
        this.error = 'Failed to load businesses.';
        this.loading = false;
      }
    });
  }

  loadTransactions(businessId: string) {
    this.loading = true;
    forkJoin({
      incomes: this.incomeService.getAll(businessId),
      expenses: this.expenseService.getAll(businessId)
    }).subscribe({
      next: ({ incomes, expenses }) => {
        const rows: TransactionRow[] = [
          ...incomes.map(i => ({ ...i, source: 'income' as const })),
          ...expenses.map(e => ({ ...e, source: 'expense' as const }))
        ];
        rows.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime());
        this.dataSource.data = rows;
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;
        this.dataSource.sortingDataAccessor = (item, property) => {
          if (property === 'date') {
            return new Date(item.date).getTime();
          }
          return (item as any)[property];
        };
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load transactions.';
        this.loading = false;
      }
    });
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.searchText = filterValue.trim().toLowerCase();
    this.applyCompositeFilter();
  }

  applyTypeFilter(type: 'all' | 'income' | 'expense'): void {
    this.selectedType = type;
    this.applyCompositeFilter();
  }

  getAmountClass(row: TransactionRow): string {
    return row.source === 'income' ? 'amount-income' : 'amount-expense';
  }

  private applyCompositeFilter(): void {
    this.dataSource.filter = JSON.stringify({
      searchText: this.searchText,
      selectedType: this.selectedType
    });
  }
}
