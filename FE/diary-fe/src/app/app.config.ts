import {
  ApplicationConfig,
  APP_INITIALIZER,
  PLATFORM_ID,
  provideZoneChangeDetection,
} from '@angular/core';
import { provideRouter } from '@angular/router';
import { isPlatformBrowser } from '@angular/common';
import {
  provideClientHydration,
  withEventReplay,
} from '@angular/platform-browser';
import { provideHttpClient, withFetch } from '@angular/common/http';
import { provideOAuthClient } from 'angular-oauth2-oidc';

import { routes } from './app.routes';
import { AuthService } from './services/auth.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideClientHydration(withEventReplay()),
    provideHttpClient(withFetch()),
    provideOAuthClient(),
    {
      provide: APP_INITIALIZER,
      useFactory: (authService: AuthService, platformId: object) =>
        () =>
          isPlatformBrowser(platformId)
            ? authService.configure()
            : Promise.resolve(false),
      deps: [AuthService, PLATFORM_ID],
      multi: true,
    },
  ],
};
