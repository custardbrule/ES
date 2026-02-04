<script lang="ts">
	import type { ModalProps, Size } from '$lib/types';

	let {
		open = false,
		size = 'md',
		title,
		children,
		footer,
		closeOnBackdrop = true,
		onClose,
		class: className = ''
	}: ModalProps = $props();

	let dialogEl: HTMLDialogElement;

	const sizes: Record<Size, string> = {
		sm: 'max-w-sm',
		md: 'max-w-md',
		lg: 'max-w-2xl'
	};

	$effect(() => {
		if (!dialogEl) return;

		if (open) {
			dialogEl.showModal();
		} else {
			dialogEl.close();
		}
	});

	function handleClose() {
		if (onClose) {
			onClose();
		}
	}

	function handleBackdropClick(event: MouseEvent) {
		if (closeOnBackdrop && event.target === dialogEl) {
			handleClose();
		}
	}
</script>

<dialog
	bind:this={dialogEl}
	class="
		fixed top-1/2 left-1/2 m-0 w-full
		-translate-x-1/2 -translate-y-1/2 rounded-lg bg-white p-0 shadow-xl backdrop:bg-black/50
		{sizes[size]}
		{className}
	"
	onclose={handleClose}
	onclick={handleBackdropClick}
>
	<!-- Header -->
	{#if title}
		<div class="flex items-center justify-between px-6 py-4">
			<h2 class="text-lg font-semibold text-gray-900">{title}</h2>
			{#if onClose}
				<button
					aria-label="Close"
					type="button"
					class="rounded p-1 text-gray-400 hover:text-gray-600 focus:ring-2 focus:ring-primary focus:outline-none"
					onclick={handleClose}
				>
					<svg class="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
						<path
							stroke-linecap="round"
							stroke-linejoin="round"
							stroke-width="2"
							d="M6 18L18 6M6 6l12 12"
						/>
					</svg>
				</button>
			{/if}
		</div>
	{/if}

	<!-- Body -->
	<div class="px-6 py-2">
		{#if children}
			{@render children()}
		{/if}
	</div>

	<!-- Footer -->
	{#if footer}
		<div class="flex items-center justify-end gap-2 px-6 py-4">
			{@render footer()}
		</div>
	{/if}
</dialog>
