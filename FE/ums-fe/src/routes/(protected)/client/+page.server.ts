import type { PageServerLoad } from './$types';
import { apiClient } from '$lib/server/api';
import { JWT_COOKIE_NAME } from '$lib/server/constants';
import type { ClientViewModel } from '$lib/types';
import { fail } from '@sveltejs/kit';

interface ClientsResponse {
	data: Record<string, unknown>[];
	totalPage: number;
	currentPage: number;
	pageSize: number;
}

export const load: PageServerLoad = async ({ cookies, url }) => {
	const token = cookies.get(JWT_COOKIE_NAME);
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
		const response = await apiClient<ClientsResponse>(`/clients?page=${page}&pageSize=${pageSize}`, { token });

		return {
			columns,
			datas: response.data,
			paging: {
				currentPage: response.currentPage,
				totalPage: response.totalPage,
				pageSize: response.pageSize
			}
		};
	} catch (err) {
		// Return fake data if API fails
		const fakeDatas: ClientViewModel[] = [
			{
				id: '1',
				clientId: 'webapp-client-001',
				displayName: 'My Web App',
				clientType: 'confidential',
				redirectUris: ['https://myapp.com/callback', 'https://myapp.com/auth'],
				postLogoutRedirectUris: ['https://myapp.com/logout'],
				permissions: ['read', 'write']
			},
			{
				id: '2',
				clientId: 'mobile-app-002',
				displayName: 'Mobile App',
				clientType: 'public',
				redirectUris: ['myapp://callback'],
				postLogoutRedirectUris: ['myapp://logout'],
				permissions: ['read']
			},
			{
				id: '3',
				clientId: 'admin-dashboard-003',
				displayName: 'Admin Dashboard',
				clientType: 'confidential',
				redirectUris: ['https://admin.example.com/callback'],
				postLogoutRedirectUris: ['https://admin.example.com/logout'],
				permissions: ['read', 'write', 'admin']
			},
			{
				id: '4',
				clientId: 'api-service-004',
				displayName: 'API Service',
				clientType: 'confidential',
				redirectUris: ['https://api.example.com/oauth'],
				postLogoutRedirectUris: [],
				permissions: ['read', 'write']
			},
			{
				id: '5',
				clientId: 'test-client-005',
				displayName: 'Test Client',
				clientType: 'public',
				redirectUris: ['http://localhost:3000/callback'],
				postLogoutRedirectUris: ['http://localhost:3000/logout'],
				permissions: ['read']
			}
		];

		return {
			columns,
			datas: fakeDatas,
			paging: { currentPage: 1, totalPage: 3, pageSize }
		};
	}
};
