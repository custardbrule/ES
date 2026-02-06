import type { PageServerLoad } from './$types';
import type { ClientViewModel } from '$lib/types';

export const load: PageServerLoad = async ({ fetch, params }) => {
	try {
		const res = await fetch(`/api/clients?id=${params.id}`);
		if (!res.ok) throw new Error('Failed to fetch client');
		const client: ClientViewModel = await res.json();
		return { client };
	} catch (err) {
		console.log(err);
		return { client: null };
	}
};
