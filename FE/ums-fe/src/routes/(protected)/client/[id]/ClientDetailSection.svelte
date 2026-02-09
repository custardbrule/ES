<script lang="ts">
	import { Button, Form } from '$lib/components';
	import { invalidate } from '$app/navigation';
	import type { ClientViewModel, UpdateClientModel, FormField, FormState } from '$lib/types';
	import type { ValidationResult } from '$lib/validator';
	import { ValidatorBuilder, rules } from '$lib/validator';

	let { client }: { client: ClientViewModel } = $props();

	let editing = $state(false);
	let loading = $state(false);
	let formModel = $state<UpdateClientModel>({
		displayName: '',
		clientType: 'confidential',
		redirectUris: [],
		postLogoutRedirectUris: []
	});
	let validationResult = $state<ValidationResult<UpdateClientModel>>();
	let formState = $state<FormState<UpdateClientModel>>({ touched: {}, submitted: false });

	$effect(() => {
		formModel = toFormModel(client);
	});

	function toFormModel(c: ClientViewModel): UpdateClientModel {
		return {
			displayName: c?.displayName ?? '',
			clientType: c?.clientType ?? 'confidential',
			redirectUris: c?.redirectUris ?? [],
			postLogoutRedirectUris: c?.postLogoutRedirectUris ?? []
		};
	}

	const validator = ValidatorBuilder.create<UpdateClientModel>((b) => {
		b.for('displayName')
			.add(rules.required, 'Display name is required')
			.add(rules.maxLength(100), 'Display name must be at most 100 characters');
		b.for('redirectUris')
			.add(rules.required, 'At least one redirect URI is required')
			.add(rules.each(rules.url), 'All redirect URIs must be valid URLs');
		b.for('postLogoutRedirectUris')
			.add(rules.required, 'At least one post logout redirect URI is required')
			.add(rules.each(rules.url), 'All post logout redirect URIs must be valid URLs');
	});

	const fields: FormField<UpdateClientModel>[] = [
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
		}
	];

	async function handleUpdate(form: UpdateClientModel) {
		loading = true;
		try {
			const res = await fetch(`/api/clients/${client.id}`, {
				method: 'PUT',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify(form)
			});
			if (!res.ok) {
				const data = await res.json();
				throw new Error(data.error || 'Failed to update client');
			}
			await invalidate('app:client');
			editing = false;
		} catch (err) {
			console.log(err);
		} finally {
			loading = false;
		}
	}

	function cancelEdit() {
		formModel = toFormModel(client);
		formState = { touched: {}, submitted: false };
		editing = false;
	}
</script>

<div class="flex w-full flex-col gap-4 rounded-md border p-4 md:w-1/2 lg:w-1/3">
	<h2 class="text-xl font-semibold underline underline-offset-4">Client detail</h2>
	<Form
		disabled={!editing}
		bind:model={formModel}
		bind:validationResult
		bind:state={formState}
		{validator}
		{fields}
		onsubmit={handleUpdate}
	/>
	<div class="flex justify-end gap-2">
		{#if editing}
			<Button variant="ghost" onclick={cancelEdit} disabled={loading}>Cancel</Button>
			<Button
				variant="secondary"
				{loading}
				disabled={!validationResult?.isValid}
				onclick={() => handleUpdate(formModel)}>Save</Button
			>
		{:else}
			<Button variant="secondary" onclick={() => (editing = true)}>Edit</Button>
		{/if}
	</div>
</div>
