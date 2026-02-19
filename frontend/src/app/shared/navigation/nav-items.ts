export interface NavItem {
	label: string;
	icon: string;
	route?: string;
	children?: NavItem[];
}

export const NAV_ITEMS: NavItem[] = [
	{ label: 'Dashboard', icon: 'dashboard', route: '/dashboard' },
	{
		label: 'Transactions',
		icon: 'receipt_long',
		route: '/transactions/history',
		children: [
			{ label: 'Income', icon: 'trending_up', route: '/transactions/income' },
			{ label: 'Expense', icon: 'trending_down', route: '/transactions/expense' },
			{ label: 'History', icon: 'history', route: '/transactions/history' }
		]
	},
	{ label: 'Reports', icon: 'bar_chart', route: '/reports' },
	{ label: 'AI Insights', icon: 'psychology', route: '/ai-insights' },
	{ label: 'Settings', icon: 'settings', route: '/settings' }
];
