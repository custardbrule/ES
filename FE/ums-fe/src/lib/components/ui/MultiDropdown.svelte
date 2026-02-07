<script lang="ts">
	import type { FormFieldOption } from '$lib/types';

	interface Props {
		options?: FormFieldOption[];
		value?: unknown[];
		placeholder?: string;
		disabled?: boolean;
		searchable?: boolean;
		id?: string;
		onchange?: (value: unknown[]) => void;
	}

	let {
		options = [],
		value = $bindable([]),
		placeholder = 'Select...',
		disabled = false,
		searchable = false,
		id,
		onchange
	}: Props = $props();

	let open = $state(false);
	let search = $state('');
	let highlightIndex = $state(-1);
	let triggerEl = $state<HTMLDivElement>();
	let listEl = $state<HTMLUListElement>();

	function isEqual(a: unknown, b: unknown): boolean {
		if (Object.is(a, b)) return true;
		if (a === null || b === null || typeof a !== 'object' || typeof b !== 'object') return false;
		return JSON.stringify(a) === JSON.stringify(b);
	}

	function isSelected(optionValue: unknown): boolean {
		return value.some((v) => isEqual(v, optionValue));
	}

	const selectedLabels = $derived(options.filter((o) => isSelected(o.value)).map((o) => o.label));

	const filtered = $derived(
		search ? options.filter((o) => o.label.toLowerCase().includes(search.toLowerCase())) : options
	);

	function toggleOption(option: FormFieldOption) {
		const idx = value.findIndex((v) => isEqual(v, option.value));
		if (idx >= 0) {
			value = value.filter((_, i) => i !== idx);
		} else {
			value = [...value, option.value];
		}
		onchange?.(value);
	}

	function removeItem(index: number) {
		value = value.filter((_, i) => i !== index);
		onchange?.(value);
	}

	function toggle() {
		if (disabled) return;
		open ? close() : openDropdown();
	}

	function openDropdown() {
		open = true;
		search = '';
		highlightIndex = -1;
	}

	function close() {
		open = false;
		search = '';
		highlightIndex = -1;
		triggerEl?.focus();
	}

	function handleKeydown(e: KeyboardEvent) {
		if (!open) {
			if (['ArrowDown', 'ArrowUp', 'Enter', ' '].includes(e.key)) {
				e.preventDefault();
				openDropdown();
			}
			return;
		}

		switch (e.key) {
			case 'ArrowDown':
				e.preventDefault();
				highlightIndex = (highlightIndex + 1) % filtered.length;
				scrollToHighlighted();
				break;
			case 'ArrowUp':
				e.preventDefault();
				highlightIndex = (highlightIndex - 1 + filtered.length) % filtered.length;
				scrollToHighlighted();
				break;
			case 'Enter':
				e.preventDefault();
				if (highlightIndex >= 0 && highlightIndex < filtered.length) {
					toggleOption(filtered[highlightIndex]);
				}
				break;
			case 'Escape':
				e.preventDefault();
				close();
				break;
		}
	}

	function scrollToHighlighted() {
		requestAnimationFrame(() => {
			const item = listEl?.children[highlightIndex] as HTMLElement | undefined;
			item?.scrollIntoView({ block: 'nearest' });
		});
	}

	function handleClickOutside(e: MouseEvent) {
		const target = e.target as Node;
		if (!triggerEl?.parentElement?.contains(target)) {
			close();
		}
	}

	$effect(() => {
		if (open) {
			document.addEventListener('mousedown', handleClickOutside);
			return () => document.removeEventListener('mousedown', handleClickOutside);
		}
	});
</script>

<div class="relative" role="listbox" aria-expanded={open} aria-multiselectable="true">
	<!-- svelte-ignore a11y_no_static_element_interactions -->
	<div
		{id}
		bind:this={triggerEl}
		role="button"
		tabindex={disabled ? -1 : 0}
		aria-disabled={disabled}
		onclick={toggle}
		onkeydown={handleKeydown}
		class="
			flex min-h-[38px] w-full cursor-pointer items-center justify-between gap-1 rounded-md border px-2 py-1.5 text-sm transition-colors
			focus:ring-1 focus:outline-none
			{disabled ? 'opacity-50' : ''}
			{open ? 'border-primary ring-1 ring-primary' : 'border-gray-300'}
		"
	>
		<div class="flex flex-1 flex-wrap gap-1">
			{#if selectedLabels.length > 0}
				{#each selectedLabels as label, i}
					<span
						class="inline-flex items-center gap-1 rounded bg-gray-100 px-2 py-0.5 text-sm text-gray-700"
					>
						{label}
						{#if !disabled}
							<button
								type="button"
								aria-label="Remove {label}"
								class="text-gray-400 hover:text-gray-600"
								onclick={(e) => {
									e.stopPropagation();
									removeItem(i);
								}}
							>
								<svg class="h-3 w-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
									<path
										stroke-linecap="round"
										stroke-linejoin="round"
										stroke-width="2"
										d="M6 18L18 6M6 6l12 12"
									/>
								</svg>
							</button>
						{/if}
					</span>
				{/each}
			{:else}
				<span class="px-1 text-gray-400">{placeholder}</span>
			{/if}
		</div>
		<svg
			class="h-4 w-4 shrink-0 text-gray-400 transition-transform {open ? 'rotate-180' : ''}"
			fill="none"
			stroke="currentColor"
			viewBox="0 0 24 24"
		>
			<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
		</svg>
	</div>

	{#if open}
		<div class="absolute z-50 mt-1 w-full rounded-md border border-gray-200 bg-white shadow-lg">
			{#if searchable}
				<div class="border-b border-gray-200 p-2">
					<!-- svelte-ignore a11y_autofocus -->
					<input
						type="text"
						bind:value={search}
						onkeydown={handleKeydown}
						autofocus
						placeholder="Search..."
						class="w-full rounded border border-gray-300 px-2 py-1 text-sm outline-none focus:border-primary"
					/>
				</div>
			{/if}

			<ul bind:this={listEl} class="max-h-48 overflow-y-auto py-1">
				{#each filtered as option, i}
					{@const selected = isSelected(option.value)}
					<li>
						<button
							type="button"
							onclick={() => toggleOption(option)}
							onmouseenter={() => (highlightIndex = i)}
							class="
								flex w-full items-center gap-2 px-3 py-2 text-left text-sm
								{selected ? 'font-medium text-primary' : ''}
								{i === highlightIndex ? 'bg-gray-100' : ''}
							"
						>
							<svg
								class="h-4 w-4 shrink-0 {selected ? 'text-primary' : 'text-gray-300'}"
								fill="none"
								stroke="currentColor"
								viewBox="0 0 24 24"
							>
								{#if selected}
									<path
										stroke-linecap="round"
										stroke-linejoin="round"
										stroke-width="2"
										d="M9 12l2 2 4-4"
									/>
									<rect x="3" y="3" width="18" height="18" rx="3" stroke-width="2" />
								{:else}
									<rect x="3" y="3" width="18" height="18" rx="3" stroke-width="2" />
								{/if}
							</svg>
							{option.label}
						</button>
					</li>
				{/each}
				{#if filtered.length === 0}
					<li class="px-3 py-2 text-sm text-gray-400">No results</li>
				{/if}
			</ul>
		</div>
	{/if}
</div>
