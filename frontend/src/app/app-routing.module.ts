import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LayoutComponent } from './layout/layout.component';
import { AuthGuard } from './core/guards/auth.guard';

const routes: Routes = [
	{
		path: '',
		component: LayoutComponent,
		children: [
			{ path: '', redirectTo: 'dashboard', pathMatch: 'full' },
			{
				path: 'dashboard',
				loadChildren: () => import('./pages/dashboard/dashboard.module').then(m => m.DashboardModule),
				canActivate: [AuthGuard]
			},
			{
				path: 'transactions',
				loadChildren: () => import('./pages/transactions/transactions.module').then(m => m.TransactionsModule),
				canActivate: [AuthGuard]
			},
			{
				path: 'reports',
				loadChildren: () => import('./pages/reports/reports.module').then(m => m.ReportsModule),
				canActivate: [AuthGuard]
			},
			{
				path: 'ai-insights',
				loadChildren: () => import('./pages/ai-insights/ai-insights.module').then(m => m.AiInsightsModule),
				canActivate: [AuthGuard]
			},
			{
				path: 'settings',
				loadChildren: () => import('./pages/settings/settings.module').then(m => m.SettingsModule),
				canActivate: [AuthGuard]
			},
			{
				path: 'setup-business',
				loadChildren: () => import('./pages/setup-business/setup-business.module').then(m => m.SetupBusinessModule),
				canActivate: [AuthGuard]
			}
		]
	},
	{ path: 'login', loadChildren: () => import('./pages/auth/auth.module').then(m => m.AuthModule) },
	{ path: 'signup', redirectTo: 'login/signup', pathMatch: 'full' },
	{ path: '**', redirectTo: 'dashboard' }
];

@NgModule({
	imports: [RouterModule.forRoot(routes)],
	exports: [RouterModule]
})
export class AppRoutingModule {}
