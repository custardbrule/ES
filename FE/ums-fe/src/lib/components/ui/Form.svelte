<script lang="ts" generics="T">
	import type { FormProps, FormField, FormState } from '$lib/types';
	import type { ValidationResult } from '$lib/validator';
	import Input from './Input.svelte';
	import ArrayInput from './ArrayInput.svelte';
	import Dropdown from './Dropdown.svelte';
	import MultiDropdown from './MultiDropdown.svelte';

	let {
		model = $bindable() as T,
		fields = [],
		validator,
		validationResult = $bindable() as ValidationResult<T>,
		state = $bindable({ touched: {}, submitted: false }) as FormState<T>,
		onsubmit,
		class: className = '',
		children
	}: FormProps<T> = $props();

	// Compute validation result when model changes
	$effect(() => {
		if (validator) {
			validationResult = validator.validate(model);
		}
	});

	function handleBlur(fieldName: keyof T) {
		state.touched[fieldName] = true;
	}

	function shouldShowError(fieldName: keyof T): boolean {
		if (!validationResult?.errors) return false;
		const hasError = !!validationResult.errors[fieldName]?.length;
		return hasError && (state.submitted || !!state.touched[fieldName]);
	}

	function getFieldError(fieldName: keyof T): string | undefined {
		return validationResult?.errors?.[fieldName]?.[0];
	}

	async function handleSubmit(e: SubmitEvent) {
		e.preventDefault();
		state.submitted = true;

		if (validationResult?.isValid && onsubmit) {
			await onsubmit(model);
		}
	}

	function updateField(field: FormField<T>, value: unknown) {
		(model as Record<string, unknown>)[field.name] = value;
	}

	function getFieldValue(field: FormField<T>): unknown {
		return (model as Record<string, unknown>)[field.name];
	}
</script>

<form onsubmit={handleSubmit} class="flex flex-col gap-4 {className}">
	{#each fields as field (field.name)}
		<div class="flex flex-col gap-1 {field.class ?? ''}">
			<label for={field.name}>{field.label}</label>

			{#if field.type === 'select' && field.options}
				<Dropdown
					id={field.name}
					value={getFieldValue(field) as string}
					onchange={(val) => { updateField(field, val); handleBlur(field.name); }}
					options={field.options}
					placeholder={field.placeholder}
					disabled={field.disabled}
				/>
			{:else if field.type === 'textarea'}
				<textarea
					id={field.name}
					value={getFieldValue(field) as string}
					oninput={(e) => updateField(field, e.currentTarget.value)}
					onblur={() => handleBlur(field.name)}
					placeholder={field.placeholder}
					disabled={field.disabled}
					class="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-primary focus:ring-1 focus:ring-primary focus:outline-none disabled:cursor-not-allowed disabled:opacity-50"
				></textarea>
			{:else if field.type === 'checkbox'}
				<input
					id={field.name}
					type="checkbox"
					checked={getFieldValue(field) as boolean}
					onchange={(e) => updateField(field, e.currentTarget.checked)}
					onblur={() => handleBlur(field.name)}
					disabled={field.disabled}
					class="h-4 w-4"
				/>
			{:else if field.type === 'multiselect' && field.options}
				<MultiDropdown
					id={field.name}
					value={getFieldValue(field) as unknown[]}
					onchange={(val) => { updateField(field, val); handleBlur(field.name); }}
					options={field.options}
					placeholder={field.placeholder}
					disabled={field.disabled}
				/>
			{:else if field.type === 'array'}
				<ArrayInput
					value={getFieldValue(field) as string[]}
					onchange={(val) => { updateField(field, val); handleBlur(field.name); }}
					placeholder={field.placeholder}
					disabled={field.disabled}
				/>
			{:else}
				<Input
					id={field.name}
					type={field.type ?? 'text'}
					value={getFieldValue(field) as string}
					oninput={(e) => updateField(field, e.currentTarget.value)}
					onblur={() => handleBlur(field.name)}
					placeholder={field.placeholder}
					disabled={field.disabled}
				/>
			{/if}

			{#if shouldShowError(field.name)}
				<span class="text-xs text-red-500">{getFieldError(field.name)}</span>
			{/if}
		</div>
	{/each}

	{#if children}
		{@render children()}
	{/if}
</form>
