import { sequence } from '@sveltejs/kit/hooks';
import type { Handle } from '@sveltejs/kit';
import { validateJWTCookie } from '$lib/server';

const authenticationHandle: Handle = ({ event, resolve }) => {
	// Get token from cookies
	const user = validateJWTCookie(event.cookies);

	// bind to locals.user
	event.locals.user = user;

	return resolve(event);
};

export const handle = sequence(authenticationHandle);
