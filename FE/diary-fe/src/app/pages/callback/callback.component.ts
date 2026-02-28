import { Component, afterNextRender } from '@angular/core';
import { AuthService } from '@src/app/services/auth.service';

@Component({
  selector: 'app-callback',
  imports: [],
  template: '',
})
export class CallbackComponent {
  constructor(private authService: AuthService) {
    afterNextRender(() => {
      this.authService.handleCallback();
    });
  }
}
