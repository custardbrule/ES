<script lang="ts">
	import { Table, Input, Button, Modal } from '$lib/components';
	import { goto, invalidateAll } from '$app/navigation';
	import type { CreateClientModel } from '$lib/types';
	import { ValidatorBuilder, rules } from '$lib/validator';

	let { data } = $props();

	let dialogRef = $state<HTMLDialogElement>();
	let createModel = $state<CreateClientModel>({
		displayName: '',
		clientType: 'confidential',
		redirectUris: [],
		postLogoutRedirectUris: [],
		permissions: []
	});
	let loading = $state(false);
	let error = $state('');

	const validator = ValidatorBuilder.create<CreateClientModel>(
		(b) =>
			b
				.for('displayName')
				.add(rules.required, 'Display name is required')
				.add(rules.minLength(3), 'Display name must be at least 3 characters')
				.add(rules.maxLength(100), 'Display name must be at most 100 characters'),
		(b) =>
			b
				.for('redirectUris')
				.add(rules.required, 'Redirect URIs are required')
				.add(rules.minItems(1), 'At least one redirect URI is required'),
		(b) =>
			b
				.for('postLogoutRedirectUris')
				.add(rules.required, 'Post logout redirect URIs are required')
				.add(rules.minItems(1), 'At least one post logout redirect URI is required'),
		(b) => b.for('permissions').add(rules.required, 'Permissions are required')
	);
	let validationResult = $derived(validator.validate(createModel));

	const handleCreate = async () => {
		if (!validationResult.isValid) return;

		loading = true;
		error = '';

		try {
			const res = await fetch('/api/clients', {
				method: 'POST',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify(createModel)
			});

			if (!res.ok) {
				const data = await res.json();
				throw new Error(data.error || 'Failed to create client');
			}

			dialogRef?.close();
			createModel = {
				displayName: '',
				clientType: 'confidential',
				redirectUris: [],
				postLogoutRedirectUris: [],
				permissions: []
			};
			await invalidateAll();
		} catch (err) {
			error = err instanceof Error ? err.message : 'An error occurred';
		} finally {
			loading = false;
		}
	};

	const onPageChange = (page: number) => {
		goto(`?page=${page}`, { keepFocus: true });
	};
</script>

<div class="flex h-full w-full flex-col gap-4 p-4 text-primary-text">
	<div class="flex items-center justify-between">
		<h1 class="text-3xl font-semibold">Clients</h1>
		<Button onclick={() => dialogRef?.showModal()} size="md" variant="secondary">Add Client</Button>
	</div>

	<!-- table -->
	<div
		style="max-height: calc(var(--content-height) - 36px - var(--space)*3);"
		class="flex-1 overflow-hidden"
	>
		<Table
			datas={data.datas}
			columns={data.columns}
			showPaging={true}
			paging={data.paging}
			{onPageChange}
		/>
	</div>
</div>

<Modal bind:dialogEl={dialogRef} title="New Client" size="lg">
	<div class="flex flex-col gap-4">
		{#if error}
			<div class="rounded bg-red-100 p-2 text-sm text-red-600">{error}</div>
		{/if}

		<div class="flex flex-col gap-1">
			<label for="displayName">Display Name</label>
			<Input id="displayName" placeholder="My Application" bind:value={createModel.displayName} />
			{#if validationResult.errors.displayName}
				<span class="text-xs text-red-500">{validationResult.errors.displayName[0]}</span>
			{/if}
		</div>

		<div class="flex flex-col gap-2">
			<label for="clientType">Client Type</label>
			<select
				id="clientType"
				bind:value={createModel.clientType}
				class="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-primary focus:ring-1 focus:ring-primary focus:outline-none"
			>
				<option value="confidential">Confidential</option>
				<option value="public">Public</option>
			</select>
		</div>

		<div class="flex flex-col gap-1">
			<label for="redirectUris">Redirect URIs</label>
			<Input
				id="redirectUris"
				placeholder="https://example.com/callback (comma separated)"
				value={createModel.redirectUris.join(', ')}
				oninput={(e) => {
					createModel.redirectUris = e.currentTarget.value
						.split(',')
						.map((s) => s.trim())
						.filter(Boolean);
				}}
			/>
			{#if validationResult.errors.redirectUris}
				<span class="text-xs text-red-500">{validationResult.errors.redirectUris[0]}</span>
			{/if}
		</div>

		<div class="flex flex-col gap-1">
			<label for="postLogoutRedirectUris">Post Logout Redirect URIs</label>
			<Input
				id="postLogoutRedirectUris"
				placeholder="https://example.com/logout (comma separated)"
				value={createModel.postLogoutRedirectUris.join(', ')}
				oninput={(e) => {
					createModel.postLogoutRedirectUris = e.currentTarget.value
						.split(',')
						.map((s) => s.trim())
						.filter(Boolean);
				}}
			/>
			{#if validationResult.errors.postLogoutRedirectUris}
				<span class="text-xs text-red-500">{validationResult.errors.postLogoutRedirectUris[0]}</span
				>
			{/if}
		</div>

		<div class="flex flex-col gap-1">
			<label for="permissions">Permissions</label>
			<Input
				id="permissions"
				placeholder="read, write (comma separated)"
				value={createModel.permissions.join(', ')}
				oninput={(e) => {
					createModel.permissions = e.currentTarget.value
						.split(',')
						.map((s) => s.trim())
						.filter(Boolean);
				}}
			/>
			{#if validationResult.errors.permissions}
				<span class="text-xs text-red-500">{validationResult.errors.permissions[0]}</span>
			{/if}
		</div>
	</div>

	{#snippet footer()}
		<Button variant="ghost" onclick={() => dialogRef?.close()} disabled={loading}>Cancel</Button>
		<Button onclick={handleCreate} {loading} disabled={!validationResult.isValid}>Create</Button>
	{/snippet}
</Modal>
