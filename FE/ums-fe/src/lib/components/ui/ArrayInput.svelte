<script lang="ts" generics="T extends string | number">
	import type { HTMLInputAttributes } from 'svelte/elements';

	interface Props extends Omit<HTMLInputAttributes, 'value' | 'onchange'> {
		value?: T[];
		placeholder?: string;
		inputClass?: string;
		onchange?: (value: T[]) => void;
	}

	let {
		id,
		value = $bindable([]) as T[],
		placeholder = '',
		inputClass = '',
		type = 'text',
		disabled = false,
		onchange,
		...rest
	}: Props = $props();

	let inputValue = $state('');

	function addItem() {
		const trimmed = inputValue.trim();
		if (!trimmed) return;

		const newValue = type === 'number' ? Number(trimmed) : trimmed;
		if (!value.includes(newValue as T)) {
			value = [...value, newValue as T];
			onchange?.(value);
		}
		inputValue = '';
	}

	function removeItem(index: number) {
		value = value.filter((_, i) => i !== index);
		onchange?.(value);
	}

	function handleKeydown(e: KeyboardEvent) {
		if (e.key === 'Enter') {
			e.preventDefault();
			addItem();
		} else if (e.key === 'Backspace' && !inputValue && value.length > 0) {
			removeItem(value.length - 1);
		}
	}
</script>

<div class="flex flex-col gap-2">
	<div
		class="flex flex-wrap items-center gap-1 rounded-md border border-gray-300 px-2 py-1.5 focus-within:border-primary focus-within:ring-1 focus-within:ring-primary {disabled
			? 'opacity-50'
			: ''} {inputClass}"
	>
		{#each value as item, index}
			<span
				class="inline-flex items-center gap-1 rounded bg-gray-100 px-2 py-0.5 text-sm text-gray-700"
			>
				{item}
				{#if !disabled}
					<button
						type="button"
						aria-label="Remove item"
						class="text-gray-400 hover:text-gray-600"
						onclick={() => removeItem(index)}
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
		{#if !disabled}
			<input
				{id}
				{...rest}
				{type}
				bind:value={inputValue}
				onkeydown={handleKeydown}
				{placeholder}
				class="min-w-20 flex-1 border-none bg-transparent text-sm outline-none"
			/>
		{/if}
	</div>
</div>
