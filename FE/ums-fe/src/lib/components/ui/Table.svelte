<script lang="ts">
	interface Column {
		label: string;
		key: string;
	}

	interface Props {
		data?: Record<string, unknown>[];
		columns?: Column[];
	}

	let { data = [], columns = [] }: Props = $props();
</script>

<div class="flex h-full w-full flex-col gap-4">
	<div class="overflow-auto">
		<table class="min-h-full min-w-full border-collapse">
			<thead>
				<tr class="bg-gray-100">
					{#each columns as column, i (column.key)}
						<th
							class="p-3 text-left whitespace-nowrap
								{i === 0 ? 'sticky left-0 z-20 bg-gray-100' : ''}"
						>
							{column.label}
						</th>
					{/each}
				</tr>
			</thead>
			<tbody>
				{#each data as row}
					<tr class="border-b hover:bg-gray-100">
						{#each columns as column, i (column.key)}
							<td
								class="p-3 whitespace-nowrap
									{i === 0 ? 'sticky left-0 z-10 bg-white' : ''}"
							>
								{row[column.key]}
							</td>
						{/each}
					</tr>
				{/each}
			</tbody>
		</table>
	</div>
	<!-- paging -->
	<div></div>
</div>
