import type { PageServerLoad } from './$types';
import type { ClientViewModel } from '$lib/types';

export const load: PageServerLoad = async ({ fetch, params }) => {
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
				redirectUris: ['sdasdasdasd', 'ádasdas'],
				postLogoutRedirectUris: ['ấgsgggggggggg'],
				permissions: ['read', 'write', 'cacs']
			}
		};
	}
};
