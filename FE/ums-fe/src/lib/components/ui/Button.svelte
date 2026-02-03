<script lang="ts">
	import type { Snippet } from 'svelte';
	import type { HTMLButtonAttributes } from 'svelte/elements';

	type Variant = 'primary' | 'secondary' | 'outline' | 'ghost' | 'danger';
	type Size = 'sm' | 'md' | 'lg';

	interface Props extends HTMLButtonAttributes {
		variant?: Variant;
		size?: Size;
		loading?: boolean;
		children?: Snippet;
		className?: string;
	}

	let {
		variant = 'primary',
		size = 'md',
		loading = false,
		disabled,
		children,
		class: className = '',
		...rest
	}: Props = $props();

	const variants: Record<Variant, string> = {
		primary: 'bg-primary text-white hover:opacity-90 active:opacity-80',
		secondary: 'bg-secondary-bg text-secondary-text hover:opacity-90 active:opacity-80',
		outline: 'border border-primary bg-transparent text-primary hover:bg-primary-bg active:opacity-80',
		ghost: 'bg-transparent text-primary hover:bg-primary-bg active:opacity-80',
		danger: 'bg-red-600 text-white hover:bg-red-700 active:bg-red-800'
	};

	const sizes: Record<Size, string> = {
		sm: 'px-3 py-1.5 text-sm',
		md: 'px-4 py-2 text-sm',
		lg: 'px-6 py-3 text-base'
	};
</script>

<button
	{...rest}
	type="button"
	disabled={disabled || loading}
	class="
		inline-flex items-center justify-center gap-2 rounded-md font-medium
		transition-colors focus:ring-2 focus:ring-primary focus:ring-offset-2 focus:outline-none
		disabled:cursor-not-allowed disabled:opacity-50
		{variants[variant]}
		{sizes[size]}
		{className}
	"
>
	{#if loading}
		<svg class="h-4 w-4 animate-spin" viewBox="0 0 24 24" fill="none">
			<circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" />
			<path
				class="opacity-75"
				fill="currentColor"
				d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
			/>
		</svg>
	{/if}
	{#if children}
		{@render children()}
	{/if}
</button>
