<script lang="ts">
	const { children } = $props();
</script>

<div class="flex h-screen w-screen flex-col bg-secondary-bg">
	<div class="flex flex-1 gap-4 p-4">
		<!-- left sidebar -->
		<div
			class="flex h-(--content-height) w-78 flex-col gap-4 overflow-auto rounded-md border bg-primary-bg p-4"
		>
			<a
				href="/client"
				class="rounded p-2 text-xl font-semibold text-gray-300 underline-offset-4 hover:bg-gray-500 hover:underline"
				>Manage clients</a
			>
		</div>

		<!-- content -->
		<div class="h-(--content-height) flex-1 overflow-auto rounded-md border bg-primary-bg">
			{@render children()}
		</div>
	</div>
	<p class="h-6 bg-primary-bg text-center text-primary-text">copyright@2026</p>
</div>
