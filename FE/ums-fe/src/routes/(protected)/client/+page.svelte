<script lang="ts">
	import { Table, Input, Button, Modal } from '$lib/components';

	let showCreateModal = $state(false);
	let createModel = $state({ name: '', description: '' });

	const handleCreate = () => {
		console.log(createModel);
		showCreateModal = false;
	}
	const columns = [
		{ label: 'Name', key: 'name' },
		{ label: 'Name', key: 'name1' },
		{ label: 'Name', key: 'name2' }
	];
	const datas = [
		{
			name: 'John Doe',
			name1: 'John Doe',
			name2: 'John Doe'
		}
	];
	const paging = { currentPage: 1, totalPage: 12 };

	const onPageChange = (page: number) => paging.currentPage = page;
</script>

<div class="flex h-full w-full flex-col gap-4 p-4 text-primary-text">
	<div class="flex justify-between items-center">
		<h1 class="text-3xl font-semibold">Clients</h1>
		<Button onclick={() => showCreateModal = true} size="md" variant="secondary">Add Client</Button>
	</div>

	<!-- table -->
	<div
		style="max-height: calc(var(--content-height) - 36px - var(--space)*3);"
		class="flex-1 overflow-hidden"
	>
		<Table {datas} {columns} showPaging={true} {paging} {onPageChange} />
	</div>
</div>

<Modal
  open={showCreateModal}
  title="New Client"
  onClose={() => showCreateModal = false}
>
  <div class="flex flex-col gap-4">
	<div class="flex flex-col gap-2">
		<label for="name">Name</label>
		<Input id="Name" placeholder="Name" bind:value={createModel.name} />
	</div>
	<div class="flex flex-col gap-2">
		<label for="description">Description</label>
		<Input id="Description" placeholder="Description" bind:value={createModel.description} />
	</div>
  </div>
  
  {#snippet footer()}
    <Button variant="ghost" onclick={() => showCreateModal = false}>Cancel</Button>
    <Button onclick={handleCreate}>Confirm</Button>
  {/snippet}
</Modal>
