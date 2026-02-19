import { Component, OnInit } from '@angular/core';
import { DashboardService } from '../../core/services/dashboard.service';
import { DashboardSummary, DashboardTrends } from '../../core/models/dashboard.model';
import { BusinessService, Business } from '../../core/services/business.service';
import { ChartConfiguration, ChartOptions } from 'chart.js';
import { Router } from '@angular/router';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  summary?: DashboardSummary;
  trends?: DashboardTrends;
  loading = true;
  error = '';

  lineChartData: ChartConfiguration<'line'>['data'] = {
    labels: [],
    datasets: []
  };
  lineChartOptions: ChartOptions<'line'> = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        position: 'bottom'
      }
    },
    scales: {
      x: {
        grid: {
          display: false
        }
      },
      y: {
        ticks: {
          callback: value => `NGN ${value}`
        }
      }
    }
  };
  lineChartType: 'line' = 'line';

  constructor(
    private dashboardService: DashboardService,
    private businessService: BusinessService,
    private router: Router
  ) {}

  ngOnInit() {
    this.businessService.getAllForCurrentUser().subscribe({
      next: (businesses: Business[]) => {
        if (businesses.length === 0) {
          this.router.navigate(['/setup-business']);
          return;
        }
        const businessId = businesses[0].id;
        localStorage.setItem('businessId', businessId);
        this.loadDashboard(businessId);
      },
      error: () => {
        this.error = 'Failed to load businesses.';
        this.loading = false;
      }
    });
  }

  loadDashboard(businessId: string) {
    this.dashboardService.getSummary(businessId).subscribe({
      next: (data) => this.summary = data,
      error: (err) => this.error = 'Failed to load summary.'
    });
    this.dashboardService.getTrends(businessId, 'monthly').subscribe({
      next: (data) => {
        this.trends = data;
        this.lineChartData = {
          labels: data.dates,
          datasets: [
            {
              data: data.in,
              label: 'Income',
              borderColor: '#16A34A',
              backgroundColor: 'rgba(22, 163, 74, 0.14)',
              tension: 0.35,
              pointRadius: 2,
              fill: true
            },
            {
              data: data.out,
              label: 'Expense',
              borderColor: '#DC2626',
              backgroundColor: 'rgba(220, 38, 38, 0.08)',
              tension: 0.35,
              pointRadius: 2,
              fill: true
            }
          ]
        };
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load trends.';
        this.loading = false;
      }
    });
  }

  getHealthClass(cashflowHealth?: string): string {
    const health = cashflowHealth?.toLowerCase() || '';
    if (health.includes('healthy') || health.includes('good')) {
      return 'health-good';
    }
    if (health.includes('warning')) {
      return 'health-warning';
    }
    if (health.includes('critical')) {
      return 'health-critical';
    }
    return 'health-critical';
  }
}
