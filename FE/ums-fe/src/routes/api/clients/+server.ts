import { json } from '@sveltejs/kit';
import type { RequestHandler } from './$types';
import { apiClient, ApiError } from '$lib/server/api';
import { JWT_COOKIE_NAME } from '$lib/server/constants';

export const GET: RequestHandler = async ({ cookies, url }) => {
	const token = cookies.get(JWT_COOKIE_NAME);
	const page = url.searchParams.get('page') || '1';

	try {
		const data = await apiClient(`/clients?page=${page}`, { token });
		return json(data);
	} catch (err) {
		if (err instanceof ApiError) {
			return json({ error: err.message, data: err.data }, { status: err.status });
		}
		return json({ error: 'Failed to fetch clients' }, { status: 500 });
	}
};

export const POST: RequestHandler = async ({ request, cookies }) => {
	const token = cookies.get(JWT_COOKIE_NAME);
	const body = await request.json();

	try {
		const data = await apiClient('/clients', {
			method: 'POST',
			body: JSON.stringify(body),
			token
		});
		return json(data, { status: 201 });
	} catch (err) {
		if (err instanceof ApiError) {
			return json({ error: err.message, data: err.data }, { status: err.status });
		}
		return json({ error: 'Failed to create client' }, { status: 500 });
	}
};
