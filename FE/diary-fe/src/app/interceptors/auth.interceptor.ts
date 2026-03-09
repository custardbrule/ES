import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { OAuthService } from 'angular-oauth2-oidc';
import { catchError, from, switchMap } from 'rxjs';
import { environment } from '@environments/environment';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const oauthService = inject(OAuthService);

  // Skip OIDC requests (discovery, token, end_session) to prevent re-entry loops
  if (req.url.startsWith(environment.oidc.issuer)) return next(req);

  const addToken = (token: string) =>
    next(req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }));

  if (oauthService.hasValidAccessToken()) {
    return addToken(oauthService.getAccessToken());
  }

  if (oauthService.getRefreshToken() && oauthService.tokenEndpoint) {
    return from(oauthService.refreshToken()).pipe(
      switchMap(() => addToken(oauthService.getAccessToken())),
      catchError(() => next(req)),
    );
  }

  return next(req);
};
