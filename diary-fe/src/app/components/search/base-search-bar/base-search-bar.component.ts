import { Component, Input, Output, EventEmitter } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SvgIconComponent } from "@src/app/components/svg-icon/svg-icon.component";

@Component({
  selector: 'app-base-search-bar',
  imports: [SvgIconComponent, FormsModule],
  templateUrl: './base-search-bar.component.html',
  styleUrl: './base-search-bar.component.scss'
})
export class BaseSearchBarComponent {
  @Input() public placeholder: string = 'Search...';
  @Input() public value: string = '';
  @Output() public valueChange = new EventEmitter<string>();
  @Output() public search = new EventEmitter<string>();

  public onInputChange(value: string): void {
    this.value = value;
    this.valueChange.emit(value);
  }

  public onSearch(): void {
    this.search.emit(this.value);
  }

  public onKeyPress(event: KeyboardEvent): void {
    if (event.key === 'Enter') {
      this.onSearch();
    }
  }
}
