<script lang="ts">
	const { children } = $props();
</script>

<div class="flex h-screen w-screen flex-col bg-secondary-bg">
	<div class="flex flex-1 gap-4 p-4">
		<!-- left sidebar -->
		<div class="flex h-full w-78 flex-col rounded-md border bg-primary-bg"></div>

		<!-- content -->
		<div class="flex-1 rounded-md border bg-primary-bg">
			{@render children()}
		</div>
	</div>
	<p class="bg-primary-bg text-center text-primary-text">copyright@2026</p>
</div>
