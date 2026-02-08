import type { PageServerLoad } from './$types';
import type { ClientViewModel } from '$lib/types';

export const load: PageServerLoad = async ({ fetch, params, depends }) => {
	depends('app:client');
	try {
		const res = await fetch(`/api/clients?id=${params.id}`);
		if (!res.ok) throw new Error('Failed to fetch client');
		const client: ClientViewModel = await res.json();
		return { client };
	} catch (err) {
		return {
			client: {
				id: 'string',
				clientId: 'string',
				displayName: 'string',
				clientType: 'confidential',
				redirectUris: [
					'http://localhost:5173/client/09134a44-c14a-41da-b5a5-b6da96eaf3ba',
					'http://localhost:5173/client/09134a44-c14a-41da-b5a5-b6da96eaf3ba'
				],
				postLogoutRedirectUris: [
					'http://localhost:5173/client/09134a44-c14a-41da-b5a5-b6da96eaf3ba'
				],
				permissions: ['read', 'write', 'cacs']
			}
		};
	}
};
