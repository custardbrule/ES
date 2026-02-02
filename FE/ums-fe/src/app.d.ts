// See https://svelte.dev/docs/kit/types#app.d.ts
// for information about these interfaces
declare global {
	interface JWTPayload {
		sub?: string;
		email?: string;
		name?: string;
		exp?: number;
		iat?: number;
		[key: string]: unknown;
	}

	namespace App {
		// interface Error {}
		interface Locals {
			user: JWTPayload | null;
		}
		// interface PageData {}
		// interface PageState {}
		// interface Platform {}
	}
}

export {};
