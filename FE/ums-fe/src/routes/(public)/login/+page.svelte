<script lang="ts">
	import { onMount } from 'svelte';
	import { Button } from '$lib/components';
	import { login } from '$lib/auth';

	let loading = $state(false);
	let tiles = $state<Array<{ x: number; y: number; w: number; h: number; color: string }>>([]);

	const CELL_SIZE = 160; // Base unit size in pixels
	const GAP = 4; // Gap between tiles for shadow visibility

	async function handleLogin() {
		loading = true;
		await login();
	}

	function generateBackground() {
		const cols = Math.ceil(window.innerWidth / CELL_SIZE);
		const rows = Math.ceil(window.innerHeight / CELL_SIZE);

		// Grid to track occupied cells
		const grid: boolean[][] = Array.from({ length: rows }, () => Array(cols).fill(false));
		const newTiles: typeof tiles = [];

		// Try to place tiles
		for (let y = 0; y < rows; y++) {
			for (let x = 0; x < cols; x++) {
				if (grid[y][x]) continue;

				// Random size (1x1, 2x1, 1x2, 2x2, 3x1, 1x3)
				const possibleSizes = [
					{ w: 1, h: 1 },
					{ w: 2, h: 1 },
					{ w: 1, h: 2 }
				];

				// Shuffle and find a size that fits
				const shuffled = possibleSizes.sort(() => Math.random() - 0.5);
				let placed = false;

				for (const size of shuffled) {
					if (canPlace(grid, x, y, size.w, size.h, cols, rows)) {
						// Mark cells as occupied
						for (let dy = 0; dy < size.h; dy++) {
							for (let dx = 0; dx < size.w; dx++) {
								grid[y + dy][x + dx] = true;
							}
						}

						newTiles.push({
							x: x * CELL_SIZE + GAP / 2,
							y: y * CELL_SIZE + GAP / 2,
							w: size.w * CELL_SIZE - GAP,
							h: size.h * CELL_SIZE - GAP,
							// color: COLORS[Math.floor(Math.random() * COLORS.length)]
							color: '#FFBBB8'
						});

						placed = true;
						break;
					}
				}

				// Fallback to 1x1 if nothing fits
				if (!placed && !grid[y][x]) {
					grid[y][x] = true;
					newTiles.push({
						x: x * CELL_SIZE + GAP / 2,
						y: y * CELL_SIZE + GAP / 2,
						w: CELL_SIZE - GAP,
						h: CELL_SIZE - GAP,
						color: '#FFBBB8'
					});
				}
			}
		}

		tiles = newTiles;
	}

	function canPlace(
		grid: boolean[][],
		x: number,
		y: number,
		w: number,
		h: number,
		cols: number,
		rows: number
	): boolean {
		if (x + w > cols || y + h > rows) return false;

		for (let dy = 0; dy < h; dy++) {
			for (let dx = 0; dx < w; dx++) {
				if (grid[y + dy][x + dx]) return false;
			}
		}
		return true;
	}

	onMount(() => {
		generateBackground();
		window.addEventListener('resize', generateBackground);
		return () => window.removeEventListener('resize', generateBackground);
	});
</script>

<div class="relative flex h-screen w-screen items-center justify-center overflow-hidden">
	<div class="absolute inset-0 -z-10">
		{#each tiles as tile}
			<div
				class="absolute cursor-pointer transition-shadow duration-200 hover:shadow-md hover:z-10"
				style="
					left: {tile.x}px;
					top: {tile.y}px;
					width: {tile.w}px;
					height: {tile.h}px;
					background-color: {tile.color};
				"
			></div>
		{/each}
	</div>
	<Button className='z-99' size="lg" variant="primary" onclick={handleLogin} {loading}>
		{loading ? 'Redirecting...' : 'Login with SSO'}
	</Button>
</div>
