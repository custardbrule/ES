import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { OAuthService } from 'angular-oauth2-oidc';
import { environment } from '@environments/environment';

export interface UserInfo {
  sub: string;
  name: string;
}

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

  private static readonly RETURN_URL_KEY = 'auth_return_url';

  async handleCallback(): Promise<void> {
    await this.oauthService.tryLogin();
    const returnUrl = sessionStorage.getItem(AuthService.RETURN_URL_KEY) ?? '/';
    sessionStorage.removeItem(AuthService.RETURN_URL_KEY);
    this.router.navigateByUrl(returnUrl);
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

  getUserInfo(): UserInfo | null {
    const claims = this.oauthService.getIdentityClaims() as UserInfo | null;
    return claims ?? null;
  }

  login(returnUrl: string = this.router.url): void {
    const safeReturnUrl = returnUrl.startsWith('/callback') ? '/' : returnUrl;
    sessionStorage.setItem(AuthService.RETURN_URL_KEY, safeReturnUrl);
    this.oauthService.initCodeFlow();
  }

  logout(): void {
    this.oauthService.logOut();
  }
}
