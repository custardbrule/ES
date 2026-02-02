import type { Cookies } from '@sveltejs/kit';
import { JWT_COOKIE_NAME, COOKIE_OPTIONS } from './constants.js';

/**
 * Set JWT token to cookie
 */
export function setJWTCookie(
	cookies: Cookies,
	token: string,
	options?: {
		maxAge?: number;
		path?: string;
		secure?: boolean;
		httpOnly?: boolean;
		sameSite?: 'strict' | 'lax' | 'none';
	}
): void {
	cookies.set(JWT_COOKIE_NAME, token, {
		...COOKIE_OPTIONS,
		...options
	});
}

/**
 * Decode JWT payload without verification (for 3rd party issued tokens)
 * Note: This does basic decoding. For production, verify the signature with the issuer's public key
 */
function decodeJWT(token: string): JWTPayload | null {
	try {
		const parts = token.split('.');
		if (parts.length !== 3) {
			return null;
		}

		const payload = parts[1];
		const decoded = JSON.parse(atob(payload.replace(/-/g, '+').replace(/_/g, '/')));
		return decoded;
	} catch {
		return null;
	}
}

/**
 * Validate JWT token and return user data
 */
export function validateJWTCookie(cookies: Cookies): JWTPayload | null {
	const token = cookies.get(JWT_COOKIE_NAME);

	if (!token) {
		return null;
	}

	const payload = decodeJWT(token);

	if (!payload) {
		return null;
	}

	// Check if token is expired
	if (payload.exp && payload.exp * 1000 < Date.now()) {
		return null;
	}

	return payload;
}

/**
 * Clear JWT cookie (logout)
 */
export function clearJWTCookie(cookies: Cookies): void {
	cookies.delete(JWT_COOKIE_NAME, { path: '/' });
}
