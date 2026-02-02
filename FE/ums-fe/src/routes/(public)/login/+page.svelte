<script>
	import { Input, Button } from '$lib/components';
</script>

<div class="flex h-screen w-screen items-center justify-center">
	<div class="flex w-full flex-col gap-4 rounded-sm p-8 sm:w-[348px] sm:border-1">
		<h1 class="text-2xl font-semibold">Login</h1>
		<div class="flex flex-col gap-2">
			<label for="account">Account</label>
			<Input />
		</div>
		<div class="flex flex-col gap-2">
			<label for="password">Password</label>
			<Input type="password" />
		</div>
		<div class="pt-2">
			<Button size="lg" variant="secondary">login</Button>
		</div>
	</div>
</div>
