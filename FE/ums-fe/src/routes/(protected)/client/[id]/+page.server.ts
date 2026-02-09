import { redirect } from '@sveltejs/kit';
import type { PageServerLoad } from './$types';
import type { ClientDetailsViewModel } from '$lib/types';

export const load: PageServerLoad = async ({ fetch, params, depends }) => {
	depends('app:client');
	const res = await fetch(`/api/clients/${params.id}`);

	if (res.status === 404) {
		redirect(302, '/client');
	}

	if (!res.ok) {
		return {
			client: {
				id: '',
				clientId: '',
				displayName: '',
				clientType: 'confidential' as const,
				consentType: '',
				redirectUris: [],
				postLogoutRedirectUris: [],
				permissions: []
			},
			roles: [],
			scopes: []
		};
	}

	const { roles, scopes, ...client }: ClientDetailsViewModel = await res.json();
	return { client, roles, scopes };
};
