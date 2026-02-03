import { json } from '@sveltejs/kit';
import type { RequestHandler } from './$types';
import { setJWTCookie, clearJWTCookie } from '$lib/server/auth';

export const POST: RequestHandler = async ({ request, cookies }) => {
	const { token } = await request.json();

	if (!token) {
		return json({ error: 'Token is required' }, { status: 400 });
	}

	setJWTCookie(cookies, token);

	return json({ success: true });
};

export const DELETE: RequestHandler = async ({ cookies }) => {
	clearJWTCookie(cookies);

	return json({ success: true });
};
