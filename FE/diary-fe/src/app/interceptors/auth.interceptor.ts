import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { OAuthService } from 'angular-oauth2-oidc';
import { from, switchMap } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const oauthService = inject(OAuthService);

  const addToken = (token: string) =>
    next(req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }));

  if (oauthService.hasValidAccessToken()) {
    return addToken(oauthService.getAccessToken());
  }

  if (oauthService.getRefreshToken()) {
    return from(oauthService.refreshToken()).pipe(
      switchMap(() => addToken(oauthService.getAccessToken())),
    );
  }

  return next(req);
};
