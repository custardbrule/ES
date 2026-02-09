import { json } from '@sveltejs/kit';
import type { RequestHandler } from './$types';
import { apiClient, ApiError } from '$lib/server/api';
import { JWT_COOKIE_NAME } from '$lib/server/constants';

export const PUT: RequestHandler = async ({ request, cookies, params }) => {
	const token = cookies.get(JWT_COOKIE_NAME);
	const body = await request.json();

	try {
		const data = await apiClient(`/api/Application/${params.id}/roles/${params.roleId}`, {
			method: 'PUT',
			body: JSON.stringify(body),
			token
		});
		return json(data);
	} catch (err) {
		if (err instanceof ApiError) {
			return json({ error: err.message, data: err.data }, { status: err.status });
		}
		return json({ error: 'Failed to update role' }, { status: 500 });
	}
};

export const DELETE: RequestHandler = async ({ cookies, params }) => {
	const token = cookies.get(JWT_COOKIE_NAME);

	try {
		await apiClient(`/api/Application/${params.id}/roles/${params.roleId}`, {
			method: 'DELETE',
			token
		});
		return new Response(null, { status: 204 });
	} catch (err) {
		if (err instanceof ApiError) {
			return json({ error: err.message, data: err.data }, { status: err.status });
		}
		return json({ error: 'Failed to delete role' }, { status: 500 });
	}
};
