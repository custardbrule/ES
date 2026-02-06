<script lang="ts" generics="T extends Record<string, unknown>">
	import type { TableProps } from '$lib/types';
	import Paging from './Paging.svelte';

	let {
		datas = [],
		columns = [],
		showPaging = false,
		paging = { currentPage: 1, totalPage: 1, pageSize: 10, pageSizeOptions: [10, 25, 50, 100] },
		trackBy,
		row,
		header,
		empty,
		onPageChange
	}: TableProps<T> = $props();
</script>

{#snippet tableRow(data: T, rowIndex: number)}
	<tr class="border-b hover:bg-gray-500">
		{#if row}
			{@render row({ data, columns, rowIndex })}
		{:else}
			{#each columns as column, colIndex (column.key)}
				<td
					class="p-3 whitespace-nowrap
						{colIndex === 0 ? 'sticky left-0 z-10 bg-white text-right' : ''}"
				>
					{data[column.key]}
				</td>
			{/each}
		{/if}
	</tr>
{/snippet}

<div class="flex h-full w-full flex-col gap-2">
	<div class="flex-1 overflow-auto rounded-md">
		<table class="min-w-full border-collapse">
			<thead class="sticky top-0 z-20">
				<tr class="bg-gray-100">
					{#if header}
						{@render header({ columns })}
					{:else}
						{#each columns as column, colIndex (column.key)}
							<th
								class="bg-gray-100 p-3 text-left whitespace-nowrap
									{colIndex === 0 ? 'sticky left-0 z-30 w-fit min-w-24 text-right' : ''}"
							>
								{column.label}
							</th>
						{/each}
					{/if}
				</tr>
			</thead>
			<tbody>
				{#if datas.length === 0}
					<tr>
						<td colspan={columns.length} class="p-8 text-center text-gray-500">
							{#if empty}
								{@render empty()}
							{:else}
								No data available
							{/if}
						</td>
					</tr>
				{:else if trackBy}
					{#each datas as data, rowIndex (data[trackBy])}
						{@render tableRow(data, rowIndex)}
					{/each}
				{:else}
					{#each datas as data, rowIndex}
						{@render tableRow(data, rowIndex)}
					{/each}
				{/if}
			</tbody>
		</table>
	</div>
	<!-- paging -->
	{#if showPaging}
		<Paging
			currentPage={paging.currentPage}
			totalPage={paging.totalPage}
			pageSize={paging.pageSize}
			pageSizeOptions={paging.pageSizeOptions}
			{onPageChange}
			onPageSizeChange={paging.onPageSizeChange}
		/>
	{/if}
</div>
