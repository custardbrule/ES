import type { PageServerLoad } from './$types';
import type { ClientViewModel } from '$lib/types';

interface ClientsResponse {
	items: ClientViewModel[];
	currentPage: number;
	pageSize: number;
	totalCount: number;
	totalPages: number;
	isPreviousPageExists: boolean;
	isNextPageExists: boolean;
}

export const load: PageServerLoad = async ({ fetch, url }) => {
	const page = Number(url.searchParams.get('page')) || 1;
	const pageSize = Number(url.searchParams.get('pageSize')) || 10;

	const columns: { label: string; key: keyof ClientViewModel }[] = [
		{ label: 'Id', key: 'id' },
		{ label: 'Client Id', key: 'clientId' },
		{ label: 'Client Type', key: 'clientType' },
		{ label: 'Display Name', key: 'displayName' },
		{ label: 'Redirect URIs', key: 'redirectUris' },
		{ label: 'Post Logout Redirect URIs', key: 'postLogoutRedirectUris' },
		{ label: 'Permissions', key: 'permissions' }
	];

	try {
		const res = await fetch(`/api/clients?page=${page}&pageSize=${pageSize}`);
		const response: ClientsResponse = await res.json();

		return {
			columns,
			datas: response.items,
			paging: {
				currentPage: response.currentPage,
				totalCount: response.totalCount,
				totalPage: response.totalPages,
				pageSize: response.pageSize
			}
		};
	} catch (err) {
		return {
			columns,
			datas: [],
			paging: { currentPage: 1, totalPage: 1, pageSize }
		};
	}
};
