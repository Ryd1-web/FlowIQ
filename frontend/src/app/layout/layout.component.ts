import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { Component } from '@angular/core';
import { Observable, map, shareReplay } from 'rxjs';

@Component({
	selector: 'app-layout',
	templateUrl: './layout.component.html',
	styleUrls: ['./layout.component.scss']
})
export class LayoutComponent {
	isHandset$: Observable<boolean>;
	isHandset = false;

	constructor(private breakpointObserver: BreakpointObserver) {
		this.isHandset$ = this.breakpointObserver.observe(Breakpoints.Handset).pipe(
			map(result => result.matches),
			shareReplay(1)
		);
		this.isHandset$.subscribe(value => {
			this.isHandset = value;
		});
	}
}
