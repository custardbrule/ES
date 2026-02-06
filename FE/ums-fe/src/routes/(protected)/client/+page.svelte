<script lang="ts">
	import { Table, Form, Button, Modal } from '$lib/components';
	import { goto, invalidateAll } from '$app/navigation';
	import type {
		ClientViewModel,
		CreateClientModel,
		FormField,
		FormState,
		IModel,
		TableColumn
	} from '$lib/types';
	import type { ValidationResult } from '$lib/validator';
	import { ValidatorBuilder, rules } from '$lib/validator';

	let { data: pageData } = $props();

	const getInitialModel = (): CreateClientModel => ({
		displayName: '',
		clientType: 'confidential',
		redirectUris: [],
		postLogoutRedirectUris: [],
		permissions: []
	});

	let dialogRef = $state<HTMLDialogElement>();
	let formModel = $state<CreateClientModel>(getInitialModel());
	let validationResult = $state<ValidationResult<CreateClientModel>>();
	let formState = $state<FormState<CreateClientModel>>({ touched: {}, submitted: false });
	let loading = $state(false);

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
			placeholder: 'read, write',
			options: [
				{ label: 'Read', value: 'read' },
				{ label: 'Write', value: 'write' },
				{ label: 'Admin', value: 'admin' }
			]
		}
	];

	const handleCreate = async (form: CreateClientModel) => {
		loading = true;

		try {
			const res = await fetch('/api/clients', {
				method: 'POST',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify(form)
			});

			if (!res.ok) {
				const data = await res.json();
				throw new Error(data.error || 'Failed to create client');
			}

			await invalidateAll();
		} catch (err) {
			console.log(err);
		} finally {
			loading = false;
			dialogRef?.close();
			formModel = getInitialModel();
			formState = { touched: {}, submitted: false };
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
			datas={pageData.datas as IModel<ClientViewModel>[]}
			columns={pageData.columns}
			showPaging={true}
			paging={pageData.paging}
			{onPageChange}
		>
			{#snippet row({
				data,
				columns,
				rowIndex
			}: {
				data: IModel<ClientViewModel>;
				columns: TableColumn<IModel<ClientViewModel>>[];
				rowIndex: number;
			})}
				<td class="sticky left-0 z-10 bg-white p-3 text-right whitespace-nowrap">
					{data.id}
				</td>
				<td class="p-3 whitespace-nowrap">
					{data.clientId}
				</td>
				<td class="p-3 whitespace-nowrap">
					{data.clientType}
				</td>
				<td class="p-3 whitespace-nowrap">
					{data.displayName}
				</td>
				<td class="p-3">
					{#each data.redirectUris as uri}
						<span class="mb-1 block w-fit rounded bg-gray-100 px-2 text-sm">{uri}</span>
					{/each}
				</td>
				<td class="p-3">
					{#each data.postLogoutRedirectUris as uri}
						<span class="mb-1 block w-fit rounded bg-gray-100 px-2 text-sm">{uri}</span>
					{/each}
				</td>
				<td class="p-3">
					<div class="flex max-w-72 flex-wrap gap-2">
						{#each data.permissions as permission}
							<span class="mb-1 flex w-fit gap-2 rounded bg-gray-100 px-2 text-sm"
								>{permission}</span
							>
						{/each}
					</div>
				</td>
			{/snippet}
		</Table>
	</div>
</div>

<Modal class={'overflow-visible'} bind:dialogEl={dialogRef} title="New Client" size="lg">
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
