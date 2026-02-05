<script lang="ts" generics="V">
	import type { FormFieldOption } from '$lib/types';

	interface Props {
		options?: FormFieldOption[];
		value?: unknown;
		placeholder?: string;
		disabled?: boolean;
		searchable?: boolean;
		id?: string;
		onchange?: (value: unknown) => void;
	}

	let {
		options = [],
		value = $bindable(),
		placeholder = 'Select...',
		disabled = false,
		searchable = false,
		id,
		onchange
	}: Props = $props();

	let open = $state(false);
	let search = $state('');
	let highlightIndex = $state(-1);
	let triggerEl = $state<HTMLButtonElement>();
	let listEl = $state<HTMLUListElement>();

	function isEqual(a: unknown, b: unknown): boolean {
		if (Object.is(a, b)) return true;
		if (a === null || b === null || typeof a !== 'object' || typeof b !== 'object') return false;
		return JSON.stringify(a) === JSON.stringify(b);
	}

	const selectedLabel = $derived(options.find((o) => isEqual(o.value, value))?.label ?? '');

	const filtered = $derived(
		search ? options.filter((o) => o.label.toLowerCase().includes(search.toLowerCase())) : options
	);

	function select(option: FormFieldOption) {
		value = option.value;
		onchange?.(option.value);
		close();
	}

	function toggle() {
		if (disabled) return;
		open ? close() : openDropdown();
	}

	function openDropdown() {
		open = true;
		search = '';
		highlightIndex = filtered.findIndex((o) => isEqual(o.value, value));
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
					select(filtered[highlightIndex]);
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

<div class="relative" role="listbox" aria-expanded={open}>
	<button
		{id}
		bind:this={triggerEl}
		type="button"
		{disabled}
		onclick={toggle}
		onkeydown={handleKeydown}
		class="
			flex w-full items-center justify-between rounded-md border px-3 py-2 text-sm transition-colors
			focus:ring-1 focus:outline-none
			disabled:cursor-not-allowed disabled:opacity-50
			{open ? 'border-primary ring-1 ring-primary' : 'border-gray-300'}
		"
	>
		<span class={selectedLabel ? '' : 'text-gray-400'}>
			{selectedLabel || placeholder}
		</span>
		<svg
			class="h-4 w-4 text-gray-400 transition-transform {open ? 'rotate-180' : ''}"
			fill="none"
			stroke="currentColor"
			viewBox="0 0 24 24"
		>
			<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
		</svg>
	</button>

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
				{#each filtered as option, i (option.value)}
					<li>
						<button
							type="button"
							onclick={() => select(option)}
							onmouseenter={() => (highlightIndex = i)}
							class="
								flex w-full items-center px-3 py-2 text-left text-sm
								{isEqual(option.value, value) ? 'font-medium text-primary' : ''}
								{i === highlightIndex ? 'bg-gray-100' : ''}
							"
						>
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
