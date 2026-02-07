<script lang="ts">
	import { Button, Form, Modal } from '$lib/components';
	import { goto, invalidateAll } from '$app/navigation';
	import type { ClientViewModel, CreateClientModel, FormField, FormState } from '$lib/types';
	import type { ValidationResult } from '$lib/validator';
	import { ValidatorBuilder, rules } from '$lib/validator';
	import { BackSvg } from '$lib/assets/icons';

	let { data } = $props();

	let client = $derived(data.client);
	let editing = $state(false);
	let loading = $state(false);
	let deleteDialogRef = $state<HTMLDialogElement>();

	// Edit form state
	let formModel = $state<CreateClientModel>(toFormModel(null));
	let validationResult = $state<ValidationResult<CreateClientModel>>();
	let formState = $state<FormState<CreateClientModel>>({ touched: {}, submitted: false });

	$effect(() => {
		formModel = toFormModel(client);
	});

	function toFormModel(c: ClientViewModel | null): CreateClientModel {
		return {
			displayName: c?.displayName ?? '',
			clientType: c?.clientType ?? 'confidential',
			redirectUris: c?.redirectUris ?? [],
			postLogoutRedirectUris: c?.postLogoutRedirectUris ?? [],
			permissions: c?.permissions ?? []
		};
	}

	const validator = ValidatorBuilder.create<CreateClientModel>((b) => {
		b.for('displayName')
			.add(rules.required, 'Display name is required')
			.add(rules.minLength(3), 'Display name must be at least 3 characters')
			.add(rules.maxLength(100), 'Display name must be at most 100 characters');
		b.for('redirectUris')
			.add(rules.required, 'At least one redirect URI is required')
			.add(rules.each(rules.url), 'All redirect URIs must be valid URLs');
		b.for('postLogoutRedirectUris')
			.add(rules.required, 'At least one post logout redirect URI is required')
			.add(rules.each(rules.url), 'All post logout redirect URIs must be valid URLs');
		b.for('permissions').add(rules.required, 'Permissions are required');
	});

	const fields: FormField<CreateClientModel>[] = [
		{ name: 'displayName', label: 'Display Name', placeholder: 'My Application' },
		{
			name: 'clientType',
			label: 'Client Type',
			type: 'select',
			options: [
				{ label: 'Confidential', value: 'confidential' },
				{ label: 'Public', value: 'public' }
			]
		},
		{
			name: 'redirectUris',
			label: 'Redirect URIs',
			type: 'array',
			placeholder: 'https://example.com/callback'
		},
		{
			name: 'postLogoutRedirectUris',
			label: 'Post Logout Redirect URIs',
			type: 'array',
			placeholder: 'https://example.com/logout'
		},
		{
			name: 'permissions',
			label: 'Permissions',
			type: 'multiselect',
			placeholder: 'Select permissions',
			options: [
				{ label: 'Read', value: 'read' },
				{ label: 'Write', value: 'write' },
				{ label: 'Admin', value: 'admin' }
			]
		}
	];

	async function handleUpdate(form: CreateClientModel) {
		if (!client) return;
		loading = true;

		try {
			const res = await fetch(`/api/clients?id=${client.id}`, {
				method: 'PUT',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify(form)
			});

			if (!res.ok) {
				const data = await res.json();
				throw new Error(data.error || 'Failed to update client');
			}

			await invalidateAll();
			editing = false;
		} catch (err) {
			console.log(err);
		} finally {
			loading = false;
		}
	}

	async function handleDelete() {
		if (!client) return;
		loading = true;

		try {
			const res = await fetch(`/api/clients?id=${client.id}`, { method: 'DELETE' });
			if (!res.ok) throw new Error('Failed to delete client');
			goto('/client');
		} catch (err) {
			console.log(err);
		} finally {
			loading = false;
			deleteDialogRef?.close();
		}
	}

	function cancelEdit() {
		formModel = toFormModel(client);
		formState = { touched: {}, submitted: false };
		editing = false;
	}
</script>

{#if client}
	<div class="flex h-full w-full">
		<div class="flex h-full w-full flex-col gap-6 p-4 text-primary-text md:w-1/2 lg:w-1/3">
			<!-- Header -->
			<div class="flex items-center justify-between">
				<div class="flex items-center gap-3">
					<a href="/client" class="rounded-md p-2 text-black hover:bg-gray-500">
						{@html BackSvg}
					</a>
					<div>
						<h1 class="text-2xl font-semibold">{client.displayName}</h1>
						<p class="text-sm text-gray-500">{client.clientId}</p>
					</div>
				</div>
				<div class="flex gap-2">
					{#if editing}
						<Button variant="ghost" onclick={cancelEdit} disabled={loading}>Cancel</Button>
						<Button
							variant="secondary"
							{loading}
							disabled={!validationResult?.isValid}
							onclick={() => handleUpdate(formModel)}>Save</Button
						>
					{:else}
						<Button variant="outline" onclick={() => (editing = true)}>Edit</Button>
						<Button variant="danger" onclick={() => deleteDialogRef?.showModal()}>Delete</Button>
					{/if}
				</div>
			</div>

			<!-- Content -->
			<Form
				disabled={!editing}
				bind:model={formModel}
				bind:validationResult
				bind:state={formState}
				{validator}
				{fields}
				onsubmit={handleUpdate}
			/>
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
		<Button variant="ghost" onclick={() => deleteDialogRef?.close()} disabled={loading}
			>Cancel</Button
		>
		<Button variant="danger" {loading} onclick={handleDelete}>Delete</Button>
	</div>
</Modal>
