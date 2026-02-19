import { Component } from '@angular/core';
import { AuthService } from '../../core/services/auth.service';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {
  phone = '';
  otp = '';
  loading = false;
  error = '';
  step = 1;

  constructor(
    private auth: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.route.queryParamMap.subscribe(params => {
      const prefillPhone = params.get('phone');
      if (prefillPhone) {
        this.phone = prefillPhone;
      }
    });
  }

  requestOtp() {
    this.loading = true;
    this.error = '';
    this.auth.requestOtp(this.phone).subscribe({
      next: () => {
        this.loading = false;
        this.step = 2;
      },
      error: () => {
        this.loading = false;
        this.error = 'Failed to request OTP.';
      }
    });
  }

  login() {
    this.loading = true;
    this.error = '';
    this.auth.login(this.phone, this.otp).subscribe({
      next: () => {
        this.loading = false;
        this.router.navigate(['/dashboard']);
      },
      error: () => {
        this.loading = false;
        this.error = 'Login failed. Check OTP.';
      }
    });
  }

  goToSignup() {
    this.router.navigate(['/login/signup']);
  }
}
