import { json } from '@sveltejs/kit';
import type { RequestHandler } from './$types';
import { apiClient, ApiError } from '$lib/server/api';
import { JWT_COOKIE_NAME } from '$lib/server/constants';

export const PUT: RequestHandler = async ({ request, cookies, params }) => {
	const token = cookies.get(JWT_COOKIE_NAME);
	const body = await request.json();

	try {
		const data = await apiClient(`/api/Application/${params.id}/scopes`, {
			method: 'PUT',
			body: JSON.stringify(body),
			token
		});
		return json(data);
	} catch (err) {
		if (err instanceof ApiError) {
			return json({ error: err.message, data: err.data }, { status: err.status });
		}
		return json({ error: 'Failed to update scopes' }, { status: 500 });
	}
};
