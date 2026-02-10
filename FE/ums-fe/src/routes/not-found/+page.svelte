<script lang="ts">
	import { Button } from '$lib/components';
	import { page } from '$app/state';

	let from = $derived(page.url.searchParams.get('from') ?? '');
</script>

<div class="flex h-full flex-col items-center justify-center gap-4">
	<h1 class="text-6xl font-bold text-gray-300">404</h1>
	<p class="text-lg text-gray-500">
		{from ? `"${from}" was not found` : 'Page not found'}
	</p>
	<a href="/">
		<Button variant="primary">Go Home</Button>
	</a>
</div>
