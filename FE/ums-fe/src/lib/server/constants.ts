export const JWT_COOKIE_NAME = 'jwt_token';

export const COOKIE_OPTIONS = {
	path: '/',
	httpOnly: true,
	secure: true,
	sameSite: 'strict' as const,
	maxAge: 60 * 60 * 24 * 7 // 7 days
} as const;
