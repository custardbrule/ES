import { Component, ElementRef, HostListener, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'app-base-dropdown',
  imports: [],
  templateUrl: './base-dropdown.component.html',
  styleUrl: './base-dropdown.component.scss'
})
export class BaseDropdownComponent {
  public isOpen = false;
  @Output() public isOpenChange = new EventEmitter<boolean>();

  constructor(private elementRef: ElementRef) {}

  public toggle(): void {
    this.isOpen = !this.isOpen;
    this.isOpenChange.emit(this.isOpen);
  }

  public open(): void {
    this.isOpen = true;
    this.isOpenChange.emit(this.isOpen);
  }

  public close(): void {
    this.isOpen = false;
    this.isOpenChange.emit(this.isOpen);
  }

  @HostListener('document:click', ['$event'])
  public onClickOutside(event: MouseEvent): void {
    if (!this.elementRef.nativeElement.contains(event.target)) {
      this.close();
    }
  }
}
