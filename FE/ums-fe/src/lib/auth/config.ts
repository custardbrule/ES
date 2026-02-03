import {
	PUBLIC_AUTH_AUTHORITY,
	PUBLIC_AUTH_CLIENT_ID,
	PUBLIC_AUTH_REDIRECT_URI,
	PUBLIC_AUTH_POST_LOGOUT_URI,
	PUBLIC_AUTH_SCOPE
} from '$env/static/public';

export const AUTH_CONFIG = {
	authority: PUBLIC_AUTH_AUTHORITY,
	clientId: PUBLIC_AUTH_CLIENT_ID,
	redirectUri: PUBLIC_AUTH_REDIRECT_URI,
	postLogoutRedirectUri: PUBLIC_AUTH_POST_LOGOUT_URI,
	scope: PUBLIC_AUTH_SCOPE,
	responseType: 'code'
} as const;

export const AUTH_ENDPOINTS = {
	authorization: `${AUTH_CONFIG.authority}/connect/authorize`,
	token: `${AUTH_CONFIG.authority}/connect/token`,
	logout: `${AUTH_CONFIG.authority}/connect/logout`,
	userinfo: `${AUTH_CONFIG.authority}/connect/userinfo`
} as const;
