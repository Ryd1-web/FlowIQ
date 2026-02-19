import { DOCUMENT } from '@angular/common';
import { Component, EventEmitter, Inject, OnInit, Output } from '@angular/core';
import { AuthService } from '../../core/services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss']
})
export class NavbarComponent implements OnInit {
  @Output() menuToggle = new EventEmitter<void>();
  isDarkMode = false;
  private readonly themeStorageKey = 'flowiq_theme';

  constructor(
    @Inject(DOCUMENT) private document: Document,
    public auth: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    const saved = localStorage.getItem(this.themeStorageKey);
    if (saved === 'dark') {
      this.applyTheme(true);
      return;
    }

    if (!saved) {
      const prefersDark = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
      this.applyTheme(prefersDark);
    }
  }

  logout() {
    this.auth.logout();
    this.router.navigate(['/login']);
  }

  toggleTheme(): void {
    this.applyTheme(!this.isDarkMode);
  }

  private applyTheme(isDark: boolean): void {
    this.isDarkMode = isDark;
    const body = this.document.body;
    if (isDark) {
      body.classList.add('dark-theme');
      localStorage.setItem(this.themeStorageKey, 'dark');
      return;
    }

    body.classList.remove('dark-theme');
    localStorage.setItem(this.themeStorageKey, 'light');
  }
}
