import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { ReportsComponent } from './reports.component';

const routes: Routes = [{ path: '', component: ReportsComponent }];

@NgModule({
	declarations: [ReportsComponent],
	imports: [CommonModule, MatCardModule, RouterModule.forChild(routes)]
})
export class ReportsModule {}
