<script lang="ts">
	import { Button, ArrayInput } from '$lib/components';
	import { invalidate } from '$app/navigation';
	import { SaveSvg } from '$lib/assets/icons';

	let { clientId, scopes }: { clientId: string; scopes: string[] } = $props();

	let loading = $state(false);
	let scopeNames = $state<string[]>([]);

	$effect(() => {
		scopeNames = [...scopes];
	});

	async function handleSave() {
		loading = true;
		try {
			const res = await fetch(`/api/clients/${clientId}/scopes`, {
				method: 'PUT',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify(scopeNames)
			});
			if (!res.ok) {
				const data = await res.json();
				throw new Error(data.error || 'Failed to update scopes');
			}
			await invalidate('app:client');
		} catch (err) {
			console.log(err);
		} finally {
			loading = false;
		}
	}
</script>

<div class="flex flex-col gap-4">
	<div class="flex items-center justify-between">
		<h2 class="text-xl font-semibold underline underline-offset-4">Scopes</h2>
		<Button class="h-8 text-primary" variant="secondary" size="sm" {loading} onclick={handleSave}>
			{@html SaveSvg}
		</Button>
	</div>
	<ArrayInput bind:value={scopeNames} placeholder="scope name" />
</div>
