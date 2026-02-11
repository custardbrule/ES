import { Injectable, ComponentRef, Type } from '@angular/core';
import { PortalService } from './portal.service';
import { ModalComponent } from '../components/modal/modal.component';

export class ModalRef<C> {
  constructor(
    private componentRef: ComponentRef<C>,
    private closeFn: () => void,
  ) {}

  get instance(): C {
    return this.componentRef.instance;
  }

  close(): void {
    this.closeFn();
  }
}

@Injectable({
  providedIn: 'root',
})
export class ModalService {
  private currentModal: ComponentRef<ModalComponent> | null = null;

  constructor(private portalService: PortalService) {}

  open<C>(component: Type<C>): ModalRef<C> {
    this.close();

    const modalRef = this.portalService.attach(ModalComponent);
    modalRef.instance.onClose = () => this.close();

    const contentRef = modalRef.instance.vcr.createComponent(component);
    this.currentModal = modalRef;

    return new ModalRef(contentRef, () => this.close());
  }

  close(): void {
    this.currentModal = null;
    this.portalService.detach();
  }
}
