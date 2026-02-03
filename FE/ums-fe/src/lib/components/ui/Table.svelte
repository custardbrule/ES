<script lang="ts">
	import type { Snippet } from 'svelte';

	interface Column {
		label: string;
		key: string;
	}

	interface Props {
		datas?: Record<string, unknown>[];
		columns?: Column[];
		row?: Snippet<[{ data: Record<string, unknown>; columns: Column[]; rowIndex: number }]>;
		header?: Snippet<[{ columns: Column[] }]>;
		empty?: Snippet;
	}

	let { datas = [], columns = [], row, header, empty }: Props = $props();
</script>

<div class="flex h-full w-full flex-col gap-4">
	<div class="overflow-auto">
		<table class="min-h-full min-w-full border-collapse">
			<thead>
				<tr class="bg-gray-100">
					{#if header}
						{@render header({ columns })}
					{:else}
						{#each columns as column, colIndex (column.key)}
							<th
								class="p-3 text-left whitespace-nowrap
									{colIndex === 0 ? 'sticky left-0 z-20 bg-gray-100' : ''}"
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
						<tr class="border-b hover:bg-gray-100">
							{#if row}
								{@render row({ data, columns, rowIndex })}
							{:else}
								{#each columns as column, colIndex (column.key)}
									<td
										class="p-3 whitespace-nowrap
											{colIndex === 0 ? 'sticky left-0 z-10 bg-white' : ''}"
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
	<div></div>
</div>
