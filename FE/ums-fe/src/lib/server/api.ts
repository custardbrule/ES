import { API_BASE_URL } from '$env/static/private';

type FetchOptions = RequestInit & {
	token?: string;
};

export class ApiError extends Error {
	status: number;
	data: unknown;

	constructor(message: string, status: number, data?: unknown) {
		super(message);
		this.name = 'ApiError';
		this.status = status;
		this.data = data;
	}
}

export async function apiClient<T>(endpoint: string, options: FetchOptions = {}): Promise<T> {
	const { token, ...fetchOptions } = options;

	const res = await fetch(`${API_BASE_URL}${endpoint}`, {
		...fetchOptions,
		headers: {
			'Content-Type': 'application/json',
			...(token && { Authorization: `Bearer ${token}` }),
			...fetchOptions.headers
		}
	});

	if (!res.ok) {
		const data = await res.json().catch(() => ({ message: 'API Error' }));
		throw new ApiError(data.message || `API Error: ${res.status}`, res.status, data);
	}

	return res.json();
}
