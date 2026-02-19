import { Component, EventEmitter, Output } from '@angular/core';
import { Router } from '@angular/router';
import { NAV_ITEMS, NavItem } from '../../shared/navigation/nav-items';

@Component({
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss']
})
export class SidebarComponent {
  @Output() itemSelected = new EventEmitter<void>();
  navItems = NAV_ITEMS;

  constructor(private router: Router) {}

  isParentActive(item: NavItem): boolean {
    if (!item.children?.length) {
      return false;
    }
    return item.children.some(child => !!child.route && this.router.url.startsWith(child.route));
  }
}
