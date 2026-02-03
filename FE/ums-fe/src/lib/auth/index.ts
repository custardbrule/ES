import { AUTH_CONFIG, AUTH_ENDPOINTS } from './config.js';
import {
	generatePKCE,
	generateState,
	storeCodeVerifier,
	storeState,
	retrieveCodeVerifier,
	retrieveState
} from './pkce.js';

export { AUTH_CONFIG, AUTH_ENDPOINTS } from './config.js';

/**
 * Initiate the OAuth login flow
 */
export async function login(): Promise<void> {
	const { verifier, challenge } = await generatePKCE();
	const state = generateState();

	storeCodeVerifier(verifier);
	storeState(state);

	const params = new URLSearchParams({
		client_id: AUTH_CONFIG.clientId,
		redirect_uri: AUTH_CONFIG.redirectUri,
		response_type: AUTH_CONFIG.responseType,
		scope: AUTH_CONFIG.scope,
		state,
		code_challenge: challenge,
		code_challenge_method: 'S256'
	});

	window.location.href = `${AUTH_ENDPOINTS.authorization}?${params}`;
}

/**
 * Exchange authorization code for tokens
 */
export async function exchangeCodeForTokens(
	code: string,
	state: string
): Promise<{ access_token: string; refresh_token?: string; id_token?: string }> {
	const storedState = retrieveState();
	if (state !== storedState) {
		throw new Error('Invalid state parameter');
	}

	const codeVerifier = retrieveCodeVerifier();
	if (!codeVerifier) {
		throw new Error('Code verifier not found');
	}

	const response = await fetch(AUTH_ENDPOINTS.token, {
		method: 'POST',
		headers: {
			'Content-Type': 'application/x-www-form-urlencoded'
		},
		body: new URLSearchParams({
			grant_type: 'authorization_code',
			client_id: AUTH_CONFIG.clientId,
			code,
			redirect_uri: AUTH_CONFIG.redirectUri,
			code_verifier: codeVerifier
		})
	});

	if (!response.ok) {
		const error = await response.text();
		throw new Error(`Token exchange failed: ${error}`);
	}

	return response.json();
}

/**
 * Logout and redirect to SSO logout
 */
export function logout(): void {
	const params = new URLSearchParams({
		post_logout_redirect_uri: AUTH_CONFIG.postLogoutRedirectUri,
		client_id: AUTH_CONFIG.clientId
	});

	window.location.href = `${AUTH_ENDPOINTS.logout}?${params}`;
}
