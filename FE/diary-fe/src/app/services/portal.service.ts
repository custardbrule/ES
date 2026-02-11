import {
  Injectable,
  ApplicationRef,
  Injector,
  EnvironmentInjector,
  Type,
  ComponentRef,
} from '@angular/core';
import { DomPortalOutlet, ComponentPortal } from '@angular/cdk/portal';

@Injectable({
  providedIn: 'root',
})
export class PortalService {
  private outlet: DomPortalOutlet | null = null;

  constructor(
    private appRef: ApplicationRef,
    private injector: Injector,
    private envInjector: EnvironmentInjector,
  ) {}

  private getOutlet(): DomPortalOutlet {
    if (!this.outlet) {
      const el = document.getElementById('app-outlet');
      if (!el) throw new Error('Portal outlet #app-outlet not found in DOM');
      this.outlet = new DomPortalOutlet(
        el,
        this.injector,
        this.appRef,
        this.envInjector,
      );
    }
    return this.outlet;
  }

  attach<C>(component: Type<C>): ComponentRef<C> {
    const outlet = this.getOutlet();
    outlet.detach();
    return outlet.attach(new ComponentPortal(component));
  }

  detach(): void {
    this.outlet?.detach();
  }
}
