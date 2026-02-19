import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../core/services/auth.service';

@Component({
	selector: 'app-settings',
	templateUrl: './settings.component.html',
	styleUrls: ['./settings.component.scss']
})
export class SettingsComponent implements OnInit {
	fullName = '';
	phoneNumber = '';
	loading = false;
	success = '';
	error = '';

	constructor(private authService: AuthService) {}

	ngOnInit(): void {
		const user = this.authService.getCurrentUserFromToken();
		this.fullName = user.fullName;
		this.phoneNumber = user.phoneNumber;
	}

	updateProfile(): void {
		this.loading = true;
		this.success = '';
		this.error = '';

		this.authService.updateProfile({
			fullName: this.fullName?.trim(),
			phoneNumber: this.phoneNumber?.trim()
		}).subscribe({
			next: (profile) => {
				this.loading = false;
				this.fullName = profile.fullName ?? profile.FullName ?? this.fullName;
				this.phoneNumber = profile.phoneNumber ?? profile.PhoneNumber ?? this.phoneNumber;
				this.success = 'Profile updated successfully.';
			},
			error: (err) => {
				this.loading = false;
				this.error = err?.error?.message || err?.error?.Message || 'Failed to update profile.';
			}
		});
	}
}
