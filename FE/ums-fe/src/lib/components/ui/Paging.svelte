<script lang="ts">
	import type { PagingProps } from '$lib/types';
	import { NextSvg, PrevSvg } from '$lib/assets/icons';
	import { Input } from '$lib/components';

	let {
		currentPage = 1,
		totalPage = 1,
		pageSize = 10,
		pageSizeOptions = [10, 25, 50, 100],
		onPageChange,
		onPageSizeChange
	}: PagingProps = $props();

	function getPageNumbers(current: number, total: number): (number | '...')[] {
		const pages: (number | '...')[] = [];
		const maxVisible = 5;

		if (total <= maxVisible + 2) {
			for (let i = 1; i <= total; i++) pages.push(i);
		} else {
			pages.push(1);

			if (current > 3) {
				pages.push('...');
			}

			const start = Math.max(2, current - 1);
			const end = Math.min(total - 1, current + 1);

			for (let i = start; i <= end; i++) {
				pages.push(i);
			}

			if (current < total - 2) {
				pages.push('...');
			}

			pages.push(total);
		}

		return pages;
	}

	function goToPage(page: number) {
		if (page >= 1 && page <= totalPage && page !== currentPage) {
			onPageChange?.(page);
		}
	}

	let pageInputValue = $state('');

	function handlePageInputKeydown(e: KeyboardEvent) {
		if (e.key === 'Enter') {
			const page = parseInt(pageInputValue);
			if (!isNaN(page)) {
				goToPage(page);
			}
			pageInputValue = '';
		}
	}
</script>

<div class="flex items-center gap-1 rounded-lg border bg-white py-2">
	<!-- Previous -->
	<button
		type="button"
		class="rounded text-sm transition-colors
				{currentPage === 1 ? 'cursor-not-allowed text-gray-300' : 'text-gray-600 hover:bg-gray-100'}"
		disabled={currentPage === 1}
		onclick={() => goToPage(currentPage - 1)}
	>
		{@html PrevSvg}
	</button>

	<!-- Page numbers -->
	{#each getPageNumbers(currentPage, totalPage) as page}
		{#if page === '...'}
			<span class="px-2 text-gray-400">...</span>
		{:else}
			<button
				type="button"
				class="min-w-8 rounded px-3 py-1 text-sm transition-colors
						{page === currentPage ? 'bg-primary text-white' : 'text-gray-600 hover:bg-gray-100'}"
				onclick={() => goToPage(page)}
			>
				{page}
			</button>
		{/if}
	{/each}

	<!-- Next -->
	<button
		type="button"
		class="rounded text-sm transition-colors
				{currentPage === totalPage
			? 'cursor-not-allowed text-gray-300'
			: 'text-gray-600 hover:bg-gray-100'}"
		disabled={currentPage === totalPage}
		onclick={() => goToPage(currentPage + 1)}
	>
		{@html NextSvg}
	</button>

	<div class="flex h-full items-center gap-4 px-4">
		<div class="flex items-center gap-2">
			<span>page</span>
			<Input
				inputClass="max-w-[48px] px-0 text-center"
				type="number"
				bind:value={pageInputValue}
				onkeydown={handlePageInputKeydown}
				min="1"
				max={totalPage}
				placeholder={String(currentPage)}
			/>
			<span>/ {totalPage}</span>
		</div>
		<span class="h-1/2 border-l"></span>
		<div class="overflow-hidden rounded">
			<select
				value={pageSize}
				onchange={(e) => onPageSizeChange?.(Number(e.currentTarget.value))}
				class="rounded border border-gray-300 px-2 py-1 text-sm outline-none focus:border-primary focus:ring-0 focus:outline-none"
			>
				{#each pageSizeOptions as size}
					<option value={size}>{size} / page</option>
				{/each}
			</select>
		</div>
	</div>
</div>
