<script lang="ts">
	const { children } = $props();
</script>

<div class="flex h-screen w-screen flex-col bg-secondary-bg">
	<div class="flex flex-1 gap-4 p-4">
		<!-- left sidebar -->
		<div class="flex h-(--content-height) overflow-auto w-78 flex-col rounded-md border bg-primary-bg"></div>

		<!-- content -->
		<div class="flex-1 h-(--content-height) overflow-auto rounded-md border bg-primary-bg">
			{@render children()}
		</div>
	</div>
	<p class="bg-primary-bg text-center text-primary-text h-6">copyright@2026</p>
</div>
