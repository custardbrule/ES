<script lang="ts">
	import type { TableProps } from '$lib/types';
	import Paging from './Paging.svelte';

	let {
		datas = [],
		columns = [],
		showPaging = false,
		paging = { currentPage: 1, totalPage: 1 },
		row,
		header,
		empty,
		onPageChange
	}: TableProps = $props();
</script>

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
									{colIndex === 0 ? 'sticky left-0 z-30 w-2xs' : ''}"
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
				{:else}
					{#each datas as data, rowIndex}
						<tr class="border-b hover:bg-gray-200">
							{#if row}
								{@render row({ data, columns, rowIndex })}
							{:else}
								{#each columns as column, colIndex (column.key)}
									<td
										class="p-3 whitespace-nowrap
											{colIndex === 0 ? 'sticky left-0 z-10 w-2xs bg-white' : ''}"
									>
										{data[column.key]}
									</td>
								{/each}
							{/if}
						</tr>
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
			{onPageChange}
		/>
	{/if}
</div>
