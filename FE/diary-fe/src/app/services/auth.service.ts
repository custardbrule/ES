import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { OAuthService } from 'angular-oauth2-oidc';
import { environment } from '@environments/environment';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  constructor(
    private oauthService: OAuthService,
    private router: Router,
  ) {}

  async configure(): Promise<boolean> {
    this.oauthService.configure({
      issuer: environment.oidc.issuer,
      redirectUri: `${window.location.origin}/callback`,
      postLogoutRedirectUri: `${window.location.origin}/signout-callback-oidc`,
      clientId: environment.oidc.clientId,
      responseType: 'code',
      scope: environment.oidc.scope,
      showDebugInformation: false,
    });
    const result = await this.oauthService.loadDiscoveryDocumentAndTryLogin();
    if (result) this.router.navigateByUrl('/');
    return result;
  }

  get isLoggedIn(): boolean {
    return this.oauthService.hasValidAccessToken();
  }

  login(): void {
    this.oauthService.initCodeFlow();
  }

  logout(): void {
    this.oauthService.logOut();
  }
}
