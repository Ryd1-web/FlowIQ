import { Component } from '@angular/core';
import { AuthService } from '../../core/services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-signup',
  templateUrl: './signup.component.html',
  styleUrls: ['./signup.component.scss']
})
export class SignupComponent {
  phone = '';
  fullName = '';
  loading = false;
  error = '';
  success = '';

  constructor(private auth: AuthService, private router: Router) {}

  signup() {
    this.loading = true;
    this.error = '';
    this.success = '';
    this.auth.signup(this.phone, this.fullName).subscribe({
      next: (res: any) => {
        this.loading = false;
        this.success = res?.message || 'Registration successful. OTP sent.';
        setTimeout(() => this.router.navigate(['/login'], { queryParams: { phone: this.phone } }), 1200);
      },
      error: (err) => {
        this.loading = false;
        this.error = err?.error?.message || err?.error?.Message || 'Registration failed.';
      }
    });
  }

  goToLogin() {
    this.router.navigate(['/login']);
  }
}
