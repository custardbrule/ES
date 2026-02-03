/**
 * Generate a random string for PKCE code verifier
 */
function generateRandomString(length: number): string {
	const charset = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~';
	const randomValues = crypto.getRandomValues(new Uint8Array(length));
	return Array.from(randomValues)
		.map((v) => charset[v % charset.length])
		.join('');
}

/**
 * Generate SHA-256 hash and return as base64url
 */
async function sha256(plain: string): Promise<ArrayBuffer> {
	const encoder = new TextEncoder();
	const data = encoder.encode(plain);
	return crypto.subtle.digest('SHA-256', data);
}

/**
 * Convert ArrayBuffer to base64url string
 */
function base64urlEncode(buffer: ArrayBuffer): string {
	const bytes = new Uint8Array(buffer);
	let binary = '';
	for (let i = 0; i < bytes.byteLength; i++) {
		binary += String.fromCharCode(bytes[i]);
	}
	return btoa(binary).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');
}

/**
 * Generate PKCE code verifier and challenge
 */
export async function generatePKCE(): Promise<{ verifier: string; challenge: string }> {
	const verifier = generateRandomString(128);
	const hashed = await sha256(verifier);
	const challenge = base64urlEncode(hashed);

	return { verifier, challenge };
}

/**
 * Generate a random state parameter for CSRF protection
 */
export function generateState(): string {
	return generateRandomString(32);
}

const STORAGE_KEYS = {
	codeVerifier: 'auth_code_verifier',
	state: 'auth_state'
} as const;

/**
 * Store PKCE verifier in sessionStorage
 */
export function storeCodeVerifier(verifier: string): void {
	sessionStorage.setItem(STORAGE_KEYS.codeVerifier, verifier);
}

/**
 * Retrieve and clear PKCE verifier from sessionStorage
 */
export function retrieveCodeVerifier(): string | null {
	const verifier = sessionStorage.getItem(STORAGE_KEYS.codeVerifier);
	sessionStorage.removeItem(STORAGE_KEYS.codeVerifier);
	return verifier;
}

/**
 * Store state in sessionStorage
 */
export function storeState(state: string): void {
	sessionStorage.setItem(STORAGE_KEYS.state, state);
}

/**
 * Retrieve and clear state from sessionStorage
 */
export function retrieveState(): string | null {
	const state = sessionStorage.getItem(STORAGE_KEYS.state);
	sessionStorage.removeItem(STORAGE_KEYS.state);
	return state;
}
