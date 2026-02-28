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

  async configure(): Promise<void> {
    this.oauthService.configure({
      issuer: environment.oidc.issuer,
      redirectUri: `${window.location.origin}/callback`,
      postLogoutRedirectUri: `${window.location.origin}/signout-callback-oidc`,
      clientId: environment.oidc.clientId,
      responseType: 'code',
      scope: environment.oidc.scope,
      showDebugInformation: false,
    });
    this.oauthService.setStorage(localStorage);
    await this.oauthService.loadDiscoveryDocument();
  }

  async handleCallback(): Promise<void> {
    await this.oauthService.tryLogin();
    this.router.navigateByUrl('/');
  }

  get isLoggedIn(): boolean {
    return this.oauthService.hasValidAccessToken();
  }

  get accessToken(): string {
    return this.oauthService.getAccessToken();
  }

  get idToken(): string {
    return this.oauthService.getIdToken();
  }

  login(): void {
    this.oauthService.initCodeFlow();
  }

  logout(): void {
    this.oauthService.logOut();
  }
}
