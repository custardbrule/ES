<script lang="ts">
	import type { HTMLTableAttributes } from 'svelte/elements';

	interface Props extends HTMLTableAttributes {
		data?: any[];
		columns?: { label: string; key: string }[];
	}

	let { data = [], columns = [], ...rest }: Props = $props();
</script>

<div class="flex flex-col gap-4">
	<div class="max-w-full overflow-x-auto">
		<!-- table -->
		<table class="w-full border-collapse">
			<thead>
				<tr class="bg-gray-100">
					{#each columns as column, i (column.key)}
						<th
							class="p-3 text-left whitespace-nowrap
                                {i === 0 ? 'sticky left-0 z-10 bg-gray-100' : ''}"
						>
							{column.label}
						</th>
					{/each}
				</tr>
			</thead>
			<tbody>
				{#each data as row}
					<tr class="border-b">
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
