export interface Expense {
  id?: string;
  date: string;
  type: string;
  category: string;
  amount: number;
  description?: string;
}
