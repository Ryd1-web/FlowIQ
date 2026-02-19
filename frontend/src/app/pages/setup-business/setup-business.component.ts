import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { BusinessService } from '../../core/services/business.service';

@Component({
  selector: 'app-setup-business',
  templateUrl: './setup-business.component.html',
  styleUrls: ['./setup-business.component.scss']
})
export class SetupBusinessComponent implements OnInit {
  name = '';
  description = '';
  category = '';
  address = '';
  loading = false;
  error = '';

  constructor(
    private businessService: BusinessService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.businessService.getAllForCurrentUser().subscribe({
      next: businesses => {
        if (businesses.length > 0) {
          this.router.navigate(['/dashboard']);
        }
      }
    });
  }

  createBusiness(): void {
    if (!this.name.trim()) {
      this.error = 'Business name is required.';
      return;
    }

    this.loading = true;
    this.error = '';

    this.businessService.create({
      name: this.name.trim(),
      description: this.description.trim(),
      category: this.category.trim(),
      address: this.address.trim()
    }).subscribe({
      next: business => {
        this.loading = false;
        localStorage.setItem('businessId', business.id);
        this.router.navigate(['/dashboard']);
      },
      error: () => {
        this.loading = false;
        this.error = 'Failed to create business profile.';
      }
    });
  }
}
