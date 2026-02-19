export interface DashboardSummary {
  todayIn: number;
  todayOut: number;
  netBalance: number;
  cashflowHealth: string;
  businessName?: string;
}

export interface DashboardTrends {
  dates: string[];
  in: number[];
  out: number[];
  net: number[];
}
