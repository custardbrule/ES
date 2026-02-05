<script lang="ts">
	import type { InputProps } from '$lib/types';

	let { value = $bindable(''), inputClass = '', type = 'text', id, ...rest }: InputProps = $props();
</script>

<div class="relative flex items-center">
	<input
		{id}
		{...rest}
		{type}
		bind:value
		class="
			w-full rounded-md border px-3 py-2 text-sm transition-colors
			focus:ring-1 focus:outline-none
			disabled:cursor-not-allowed disabled:opacity-50
			{inputClass}
		"
	/>
</div>

<style lang="scss">
	input {
		&::-webkit-outer-spin-button,
		&::-webkit-inner-spin-button {
			-webkit-appearance: none;
			margin: 0;
		}

		/* Firefox */
		&[type='number'] {
			-moz-appearance: textfield;
			appearance: none;
		}
	}
</style>
