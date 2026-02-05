<script lang="ts">
	import { Table, Form, Button, Modal } from '$lib/components';
	import { goto, invalidateAll } from '$app/navigation';
	import type { CreateClientModel, FormField, FormState } from '$lib/types';
	import type { ValidationResult } from '$lib/validator';
	import { ValidatorBuilder, rules } from '$lib/validator';

	let { data } = $props();

	// Form model with string fields for UI
	interface ClientFormModel {
		displayName: string;
		clientType: string;
		redirectUris: string;
		postLogoutRedirectUris: string;
		permissions: string;
	}

	const initialModel: ClientFormModel = {
		displayName: '',
		clientType: 'confidential',
		redirectUris: '',
		postLogoutRedirectUris: '',
		permissions: ''
	};

	let dialogRef = $state<HTMLDialogElement>();
	let formModel = $state<ClientFormModel>({ ...initialModel });
	let validationResult = $state<ValidationResult<ClientFormModel>>();
	let formState = $state<FormState<ClientFormModel>>({ touched: {}, submitted: false });
	let loading = $state(false);

	const validator = ValidatorBuilder.create<ClientFormModel>((b) => {
		b.for('displayName')
			.add(rules.required, 'Display name is required')
			.add(rules.minLength(3), 'Display name must be at least 3 characters')
			.add(rules.maxLength(100), 'Display name must be at most 100 characters');
		b.for('redirectUris').add(rules.required, 'At least one redirect URI is required');
		b.for('postLogoutRedirectUris').add(
			rules.required,
			'At least one post logout redirect URI is required'
		);
		b.for('permissions').add(rules.required, 'Permissions are required');
	});

	const fields: FormField<ClientFormModel>[] = [
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
			placeholder: 'https://example.com/callback (comma separated)'
		},
		{
			name: 'postLogoutRedirectUris',
			label: 'Post Logout Redirect URIs',
			placeholder: 'https://example.com/logout (comma separated)'
		},
		{ name: 'permissions', label: 'Permissions', placeholder: 'read, write (comma separated)' }
	];

	const handleCreate = async (form: ClientFormModel) => {
		loading = true;
		const toArray = (str: string) =>
			str
				.split(',')
				.map((s) => s.trim())
				.filter(Boolean);

		const transform: CreateClientModel = {
			displayName: form.displayName,
			clientType: form.clientType as 'confidential' | 'public',
			redirectUris: toArray(form.redirectUris),
			postLogoutRedirectUris: toArray(form.postLogoutRedirectUris),
			permissions: toArray(form.permissions)
		};

		try {
			const res = await fetch('/api/clients', {
				method: 'POST',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify(transform)
			});

			if (!res.ok) {
				const data = await res.json();
				throw new Error(data.error || 'Failed to create client');
			}

			formModel = { ...initialModel };
			formState = { touched: {}, submitted: false };
			await invalidateAll();
		} catch (err) {
			console.log(err);
		} finally {
			loading = false;
			dialogRef?.close();
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
	<Form
		bind:model={formModel}
		bind:validationResult
		bind:state={formState}
		{validator}
		{fields}
		onsubmit={handleCreate}
	></Form>
	<div class="flex justify-end gap-2 pt-4">
		<Button variant="ghost" onclick={() => dialogRef?.close()} disabled={loading}>Cancel</Button>
		<Button {loading} disabled={!validationResult?.isValid} onclick={() => handleCreate(formModel)}
			>Create</Button
		>
	</div>
</Modal>
