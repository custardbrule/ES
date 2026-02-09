<script lang="ts">
	import { Button, Modal } from '$lib/components';
	import { goto } from '$app/navigation';
	import { BackSvg } from '$lib/assets/icons';
	import ClientDetailSection from './ClientDetailSection.svelte';
	import RolesSection from './RolesSection.svelte';
	import ScopesSection from './ScopesSection.svelte';

	let { data } = $props();

	let client = $derived(data.client);
	let roles = $derived(data.roles);
	let scopes = $derived(data.scopes.map((s) => s.name));
	let loading = $state(false);
	let deleteDialogRef = $state<HTMLDialogElement>();

	async function handleDelete() {
		if (!client) return;
		loading = true;
		try {
			const res = await fetch(`/api/clients/${client.id}`, { method: 'DELETE' });
			if (!res.ok) throw new Error('Failed to delete client');
			goto('/client');
		} catch (err) {
			console.log(err);
		} finally {
			loading = false;
			deleteDialogRef?.close();
		}
	}
</script>

{#if client}
	<div class="flex h-full w-full flex-col p-4">
		<!-- Header -->
		<div class="flex items-center justify-between p-4">
			<div class="flex items-center gap-3">
				<a href="/client" class="rounded-md p-2 text-black hover:bg-gray-500">
					<span class="inline-flex h-8 w-8">
						{@html BackSvg}
					</span>
				</a>
				<div>
					<h1 class="text-3xl font-semibold text-primary-text">{client.displayName}</h1>
					<p class="text-sm text-gray-500">{client.clientId}</p>
				</div>
			</div>
			<Button variant="danger" onclick={() => deleteDialogRef?.showModal()}>Delete</Button>
		</div>
		<div class="flex w-full flex-1 flex-col gap-8 p-4 text-primary-text md:flex-row">
			<ClientDetailSection {client} />

			<div class="flex flex-1 flex-col gap-4 rounded-md border p-4">
				<ScopesSection clientId={client.id} {scopes} />
				<div class="w-full border-t"></div>
				<RolesSection clientId={client.id} {roles} />
			</div>
		</div>
	</div>
{:else}
	<div class="flex h-full items-center justify-center text-gray-500">
		<p>Client not found</p>
	</div>
{/if}

<!-- Delete confirmation -->
<Modal bind:dialogEl={deleteDialogRef} title="Delete Client" size="sm">
	<p class="text-sm text-gray-600">
		Are you sure you want to delete <strong>{client?.displayName}</strong>? This action cannot be
		undone.
	</p>
	<div class="flex justify-end gap-2 pt-4">
		<Button variant="ghost" onclick={() => deleteDialogRef?.close()} disabled={loading}>
			Cancel
		</Button>
		<Button variant="danger" {loading} onclick={handleDelete}>Delete</Button>
	</div>
</Modal>
