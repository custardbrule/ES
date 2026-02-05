import type { PageServerLoad } from './$types';
import { apiClient } from '$lib/server/api';
import { JWT_COOKIE_NAME } from '$lib/server/constants';
import { fail } from '@sveltejs/kit';


interface ClientsResponse {
	data: Record<string, unknown>[];
	totalPage: number;
	currentPage: number;
}

export const load: PageServerLoad = async ({ cookies, url }) => {
	const token = cookies.get(JWT_COOKIE_NAME);
	const page = Number(url.searchParams.get('page')) || 1;

	const columns = [
		{ label: 'Name', key: 'name' },
		{ label: 'Description', key: 'description' }
	];

	try {
		const response = await apiClient<ClientsResponse>(`/clients?page=${page}`, { token });

		return {
			columns,
			datas: response.data,
			paging: {
				currentPage: response.currentPage,
				totalPage: response.totalPage
			}
		};
	} catch (err) {
		// Return empty data if API fails
		return {
			columns,
			datas: [],
			paging: { currentPage: 1, totalPage: 1 }
		};
	}
};