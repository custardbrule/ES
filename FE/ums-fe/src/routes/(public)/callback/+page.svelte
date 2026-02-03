<script lang="ts">
	import { onMount } from 'svelte';
	import { goto } from '$app/navigation';
	import { page } from '$app/state';
	import { exchangeCodeForTokens } from '$lib/auth';

	let error = $state<string | null>(null);
	let loading = $state(true);

	onMount(async () => {
		const code = page.url.searchParams.get('code');
		const state = page.url.searchParams.get('state');
		const errorParam = page.url.searchParams.get('error');
		const errorDescription = page.url.searchParams.get('error_description');

		if (errorParam) {
			error = errorDescription || errorParam;
			loading = false;
			return;
		}

		if (!code || !state) {
			error = 'Missing authorization code or state';
			loading = false;
			return;
		}

		try {
			const tokens = await exchangeCodeForTokens(code, state);

			// Send token to server to store in HTTP-only cookie
			const response = await fetch('/api/auth/session', {
				method: 'POST',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify({ token: tokens.access_token })
			});

			if (!response.ok) {
				throw new Error('Failed to create session');
			}

			// Redirect to home/dashboard
			goto('/');
		} catch (err) {
			error = err instanceof Error ? err.message : 'Authentication failed';
			loading = false;
		}
	});
</script>

<div class="flex h-screen w-screen items-center justify-center bg-secondary-bg">
	<div class="text-center">
		{#if loading}
			<div class="flex flex-col items-center gap-4">
				<svg class="h-8 w-8 animate-spin text-primary" viewBox="0 0 24 24" fill="none">
					<circle
						class="opacity-25"
						cx="12"
						cy="12"
						r="10"
						stroke="currentColor"
						stroke-width="4"
					/>
					<path
						class="opacity-75"
						fill="currentColor"
						d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
					/>
				</svg>
				<p class="text-primary">Completing sign in...</p>
			</div>
		{:else if error}
			<div class="flex flex-col items-center gap-4">
				<p class="text-red-600">Authentication Error</p>
				<p class="text-sm text-gray-600">{error}</p>
				<a href="/login" class="text-primary underline">Back to login</a>
			</div>
		{/if}
	</div>
</div>
