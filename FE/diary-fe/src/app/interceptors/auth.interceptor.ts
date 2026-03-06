import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { environment } from '@environments/environment';
import { OAuthService } from 'angular-oauth2-oidc';
import { catchError, from, switchMap } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const oauthService = inject(OAuthService);

  if (req.url.startsWith(environment.oidc.issuer)) {
    return next(req);
  }

  const addToken = (token: string) =>
    next(req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }));

  if (oauthService.hasValidAccessToken()) {
    return addToken(oauthService.getAccessToken());
  }

  if (oauthService.getRefreshToken()) {
    return from(oauthService.refreshToken()).pipe(
      switchMap(() => addToken(oauthService.getAccessToken())),
      catchError(() => next(req)),
    );
  }

  return next(req);
};
