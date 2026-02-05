import { json } from '@sveltejs/kit';
import type { RequestHandler } from './$types';
import { apiClient, ApiError } from '$lib/server/api';
import { JWT_COOKIE_NAME } from '$lib/server/constants';

// Whitelist of allowed API paths (security)
const ALLOWED_PATHS = [
	'/clients',
	'/users',
	'/roles',
	'/permissions',
	'/scopes'
];

function isPathAllowed(path: string): boolean {
	return ALLOWED_PATHS.some((allowed) => path === allowed || path.startsWith(`${allowed}/`));
}

async function handleRequest(
	method: string,
	path: string,
	token: string | undefined,
	searchParams: URLSearchParams,
	body?: unknown
) {
	const queryString = searchParams.toString();
	const endpoint = queryString ? `${path}?${queryString}` : path;

	const options: RequestInit & { token?: string } = {
		method,
		token
	};

	if (body && ['POST', 'PUT', 'PATCH'].includes(method)) {
		options.body = JSON.stringify(body);
	}

	return apiClient(endpoint, options);
}

export const GET: RequestHandler = async ({ params, cookies, url }) => {
	const path = `/${params.path}`;

	if (!isPathAllowed(path)) {
		return json({ error: 'Endpoint not allowed' }, { status: 403 });
	}

	const token = cookies.get(JWT_COOKIE_NAME);

	try {
		const data = await handleRequest('GET', path, token, url.searchParams);
		return json(data);
	} catch (err) {
		if (err instanceof ApiError) {
			return json({ error: err.message, data: err.data }, { status: err.status });
		}
		return json({ error: 'Server error' }, { status: 500 });
	}
};

export const POST: RequestHandler = async ({ params, cookies, url, request }) => {
	const path = `/${params.path}`;

	if (!isPathAllowed(path)) {
		return json({ error: 'Endpoint not allowed' }, { status: 403 });
	}

	const token = cookies.get(JWT_COOKIE_NAME);
	const body = await request.json().catch(() => ({}));

	try {
		const data = await handleRequest('POST', path, token, url.searchParams, body);
		return json(data, { status: 201 });
	} catch (err) {
		if (err instanceof ApiError) {
			return json({ error: err.message, data: err.data }, { status: err.status });
		}
		return json({ error: 'Server error' }, { status: 500 });
	}
};

export const PUT: RequestHandler = async ({ params, cookies, url, request }) => {
	const path = `/${params.path}`;

	if (!isPathAllowed(path)) {
		return json({ error: 'Endpoint not allowed' }, { status: 403 });
	}

	const token = cookies.get(JWT_COOKIE_NAME);
	const body = await request.json().catch(() => ({}));

	try {
		const data = await handleRequest('PUT', path, token, url.searchParams, body);
		return json(data);
	} catch (err) {
		if (err instanceof ApiError) {
			return json({ error: err.message, data: err.data }, { status: err.status });
		}
		return json({ error: 'Server error' }, { status: 500 });
	}
};

export const PATCH: RequestHandler = async ({ params, cookies, url, request }) => {
	const path = `/${params.path}`;

	if (!isPathAllowed(path)) {
		return json({ error: 'Endpoint not allowed' }, { status: 403 });
	}

	const token = cookies.get(JWT_COOKIE_NAME);
	const body = await request.json().catch(() => ({}));

	try {
		const data = await handleRequest('PATCH', path, token, url.searchParams, body);
		return json(data);
	} catch (err) {
		if (err instanceof ApiError) {
			return json({ error: err.message, data: err.data }, { status: err.status });
		}
		return json({ error: 'Server error' }, { status: 500 });
	}
};

export const DELETE: RequestHandler = async ({ params, cookies, url }) => {
	const path = `/${params.path}`;

	if (!isPathAllowed(path)) {
		return json({ error: 'Endpoint not allowed' }, { status: 403 });
	}

	const token = cookies.get(JWT_COOKIE_NAME);

	try {
		const data = await handleRequest('DELETE', path, token, url.searchParams);
		return json(data);
	} catch (err) {
		if (err instanceof ApiError) {
			return json({ error: err.message, data: err.data }, { status: err.status });
		}
		return json({ error: 'Server error' }, { status: 500 });
	}
};
