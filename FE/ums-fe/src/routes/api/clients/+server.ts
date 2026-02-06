import { json } from '@sveltejs/kit';
import type { RequestHandler } from './$types';
import { apiClient, ApiError } from '$lib/server/api';
import { JWT_COOKIE_NAME } from '$lib/server/constants';

export const GET: RequestHandler = async ({ cookies, url }) => {
	const token = cookies.get(JWT_COOKIE_NAME);
	const id = url.searchParams.get('id');

	try {
		if (id) {
			const data = await apiClient(`/api/Application/${id}`, { token });
			return json(data);
		}

		const page = url.searchParams.get('page') || '1';
		const data = await apiClient(`/api/Application?page=${page}`, { token });
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
		const data = await apiClient('/api/Application', {
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

export const PUT: RequestHandler = async ({ request, cookies, url }) => {
	const token = cookies.get(JWT_COOKIE_NAME);
	const id = url.searchParams.get('id');
	const body = await request.json();

	try {
		const data = await apiClient(`/api/Application/${id}`, {
			method: 'PUT',
			body: JSON.stringify(body),
			token
		});
		return json(data);
	} catch (err) {
		if (err instanceof ApiError) {
			return json({ error: err.message, data: err.data }, { status: err.status });
		}
		return json({ error: 'Failed to update client' }, { status: 500 });
	}
};

export const DELETE: RequestHandler = async ({ cookies, url }) => {
	const token = cookies.get(JWT_COOKIE_NAME);
	const id = url.searchParams.get('id');

	try {
		await apiClient(`/api/Application/${id}`, { method: 'DELETE', token });
		return new Response(null, { status: 204 });
	} catch (err) {
		if (err instanceof ApiError) {
			return json({ error: err.message, data: err.data }, { status: err.status });
		}
		return json({ error: 'Failed to delete client' }, { status: 500 });
	}
};
