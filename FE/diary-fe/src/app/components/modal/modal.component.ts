import {
  Component,
  Input,
  Output,
  EventEmitter,
  ElementRef,
  HostListener,
  OnDestroy,
  afterNextRender,
} from '@angular/core';

@Component({
  selector: 'app-modal',
  imports: [],
  templateUrl: './modal.component.html',
  styleUrl: './modal.component.scss',
})
export class ModalComponent implements OnDestroy {
  @Input() isOpen = false;
  @Output() isOpenChange = new EventEmitter<boolean>();

  constructor(private elementRef: ElementRef) {
    afterNextRender(() => {
      const outlet = document.getElementById('app-outlet');
      if (outlet) {
        outlet.appendChild(this.elementRef.nativeElement);
      }
    });
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.isOpen) {
      this.close();
    }
  }

  open(): void {
    this.isOpen = true;
    this.isOpenChange.emit(this.isOpen);
  }

  close(): void {
    this.isOpen = false;
    this.isOpenChange.emit(this.isOpen);
  }

  ngOnDestroy(): void {
    this.elementRef.nativeElement.remove();
  }
}
