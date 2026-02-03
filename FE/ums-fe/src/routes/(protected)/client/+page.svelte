<script>
	import { Table } from '$lib/components';
</script>

<div class="flex h-full w-full flex-col gap-4 p-4 text-primary-text">
	<h1 class="text-3xl font-semibold">Clients</h1>

	<!-- table -->
	<Table
		data={[
			{
				name: 'John Doe',
				name1: 'John Doe',
				name2: 'John Doe',
				nam3e: 'John Doe',
				name4: 'John Doe',
				name5: 'John Doe',
				nam6e: 'John Doe',
				nam7e: 'John Doe',
				name8: 'John Doe',
				email: 'John Doe'
			}
		]}
		columns={[
			{ label: 'Name', key: 'name' },
			{ label: 'Name', key: 'name1' },
			{ label: 'Name', key: 'name2' },
			{ label: 'Name', key: 'nam3e' },
			{ label: 'Name', key: 'name4' },
			{ label: 'Name', key: 'name5' },
			{ label: 'Name', key: 'nam6e' },
			{ label: 'Name', key: 'nam7e' },
			{ label: 'Name', key: 'name8' },
			{ label: 'Email', key: 'email' }
		]}
	/>
</div>
