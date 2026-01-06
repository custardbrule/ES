import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SvgIconComponent } from "@src/app/components/svg-icon/svg-icon.component";
import { BaseDropdownComponent } from "@src/app/components/dropdowns/base-dropdown/base-dropdown.component";
import { BaseSearchBarComponent } from "@src/app/components/search/base-search-bar/base-search-bar.component";

@Component({
  selector: 'app-layout',
  imports: [RouterOutlet, SvgIconComponent, BaseDropdownComponent, BaseSearchBarComponent],
  templateUrl: './layout.component.html',
  styleUrl: './layout.component.scss'
})
export class LayoutComponent {
}
