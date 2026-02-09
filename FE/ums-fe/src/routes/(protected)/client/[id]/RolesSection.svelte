<script lang="ts">
	import { Button, Form, Modal } from '$lib/components';
	import { invalidate } from '$app/navigation';
	import type {
		ClientRoleViewModel,
		UpdateClientRoleModel,
		CreateClientRoleModel,
		FormField
	} from '$lib/types';
	import { ValidatorBuilder, rules } from '$lib/validator';
	import { PlusSvg, SaveSvg, DeleteSvg } from '$lib/assets/icons';

	let { clientId, roles }: { clientId: string; roles: ClientRoleViewModel[] } = $props();

	let loading = $state(false);
	let createDialogRef = $state<HTMLDialogElement>();
	let deleteDialogRef = $state<HTMLDialogElement>();
	let deleteIndex = $state<number | null>(null);
	let createModel = $state<CreateClientRoleModel>({ name: '', description: '', scopes: [] });
	let roleModels = $state<UpdateClientRoleModel[]>([]);

	$effect(() => {
		roleModels = roles.map((r) => ({ name: r.name, description: r.description, scopes: r.scopes }));
	});

	const validator = ValidatorBuilder.create<UpdateClientRoleModel>((b) => {
		b.for('name').add(rules.required, 'Name is required');
	});

	const fields: FormField<UpdateClientRoleModel>[] = [
		{ name: 'name', label: 'Name', placeholder: 'Admin' },
		{ name: 'description', label: 'Description', placeholder: 'description' },
		{ name: 'scopes', label: 'Scopes', type: 'array', placeholder: 'read, write' }
	];

	async function handleCreate(form: CreateClientRoleModel) {
		loading = true;
		try {
			const res = await fetch(`/api/clients/${clientId}/roles`, {
				method: 'POST',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify(form)
			});
			if (!res.ok) {
				const data = await res.json();
				throw new Error(data.error || 'Failed to create role');
			}
			await invalidate('app:client');
			createModel = { name: '', description: '', scopes: [] };
			createDialogRef?.close();
		} catch (err) {
			console.log(err);
		} finally {
			loading = false;
		}
	}

	async function handleUpdate(index: number, form: UpdateClientRoleModel) {
		const role = roles[index];
		if (!role) return;
		loading = true;
		try {
			const res = await fetch(`/api/clients/${clientId}/roles/${role.id}`, {
				method: 'PUT',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify(form)
			});
			if (!res.ok) {
				const data = await res.json();
				throw new Error(data.error || 'Failed to update role');
			}
			await invalidate('app:client');
		} catch (err) {
			console.log(err);
		} finally {
			loading = false;
		}
	}

	async function handleDelete() {
		if (deleteIndex === null) return;
		const role = roles[deleteIndex];
		if (!role) return;
		loading = true;
		try {
			const res = await fetch(`/api/clients/${clientId}/roles/${role.id}`, { method: 'DELETE' });
			if (!res.ok) throw new Error('Failed to delete role');
			await invalidate('app:client');
		} catch (err) {
			console.log(err);
		} finally {
			loading = false;
			deleteIndex = null;
			deleteDialogRef?.close();
		}
	}
</script>

<div class="flex flex-col gap-4">
	<div class="flex items-center justify-between">
		<h2 class="text-xl font-semibold underline underline-offset-4">Roles</h2>
		<Button variant="secondary" onclick={() => createDialogRef?.showModal()}>
			<span class="inline-flex h-5 w-5">{@html PlusSvg}</span> New role
		</Button>
	</div>
	<div class="flex flex-wrap gap-x-12 gap-y-8">
		{#each roleModels as _, i}
			<Form
				bind:model={roleModels[i]}
				{validator}
				{fields}
				onsubmit={(form) => handleUpdate(i, form)}
				class="max-w-xs min-w-xs flex-auto"
			>
				<div class="flex justify-end gap-2">
					<Button class="h-8" variant="danger" size="sm" onclick={() => { deleteIndex = i; deleteDialogRef?.showModal(); }}
						>{@html DeleteSvg}</Button
					>
					<Button class="h-8 text-primary" type="submit" variant="secondary" size="sm"
						>{@html SaveSvg}</Button
					>
				</div>
			</Form>
		{/each}
	</div>
</div>

<Modal bind:dialogEl={deleteDialogRef} title="Delete Role" size="sm">
	<p class="text-sm text-gray-600">
		Are you sure you want to delete <strong>{deleteIndex !== null ? roles[deleteIndex]?.name : ''}</strong>? This action cannot be undone.
	</p>
	<div class="flex justify-end gap-2 pt-4">
		<Button variant="ghost" onclick={() => { deleteIndex = null; deleteDialogRef?.close(); }} disabled={loading}>
			Cancel
		</Button>
		<Button variant="danger" {loading} onclick={handleDelete}>Delete</Button>
	</div>
</Modal>

<Modal bind:dialogEl={createDialogRef} title="New Role" size="sm">
	<Form bind:model={createModel} {validator} {fields} onsubmit={handleCreate}>
		<div class="flex justify-end gap-2">
			<Button variant="ghost" onclick={() => createDialogRef?.close()} disabled={loading}>
				Cancel
			</Button>
			<Button type="submit" variant="primary" {loading}>Create</Button>
		</div>
	</Form>
</Modal>
