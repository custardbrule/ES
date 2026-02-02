<script lang="ts">
	import type { HTMLInputAttributes } from 'svelte/elements';

	interface Props extends HTMLInputAttributes {
		value?: string;
		inputClass?: string;
	}

	let { value = $bindable(''), inputClass = '', type = 'text', id, ...rest }: Props = $props();
</script>

<div class="relative flex items-center">
	<input
		{id}
		{...rest}
		{type}
		bind:value
		class="
			w-full rounded-md border px-3 py-2 text-sm transition-colors
			focus:ring-2 focus:outline-none
			disabled:cursor-not-allowed disabled:opacity-50
			{inputClass}
		"
	/>
</div>
