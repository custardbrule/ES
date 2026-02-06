import type { Snippet } from 'svelte';
import type {
	HTMLButtonAttributes,
	HTMLFormAttributes,
	HTMLInputAttributes
} from 'svelte/elements';
import type { ValidatorBuilder, ValidationResult } from '$lib/validator';

// ============================================
// Shared Types
// ============================================

export type Size = 'sm' | 'md' | 'lg';

// ============================================
// Form Types
// ============================================

export type FormFieldType =
	| 'text'
	| 'email'
	| 'password'
	| 'number'
	| 'textarea'
	| 'select'
	| 'checkbox'
	| 'array'
	| 'multiselect';

export interface FormFieldOption {
	label: string;
	value: unknown;
}

export interface FormField<T> {
	name: keyof T & string;
	label: string;
	type?: FormFieldType;
	placeholder?: string;
	options?: FormFieldOption[]; // for select
	disabled?: boolean;
	class?: string;
}

export interface FormState<T> {
	touched: Partial<Record<keyof T, boolean>>;
	submitted: boolean;
}

export interface FormProps<T> extends Omit<HTMLFormAttributes, 'onsubmit'> {
	model: T;
	fields: FormField<T>[];
	validator?: ValidatorBuilder<T>;
	validationResult?: ValidationResult<T>;
	state?: FormState<T>;
	onsubmit?: (model: T) => void | Promise<void>;
	class?: string;
	children?: Snippet;
}

// ============================================
// Button Types
// ============================================

export type ButtonVariant = 'primary' | 'secondary' | 'outline' | 'ghost' | 'danger';

export interface ButtonProps extends HTMLButtonAttributes {
	variant?: ButtonVariant;
	size?: Size;
	loading?: boolean;
	children?: Snippet;
	className?: string;
}

// ============================================
// Input Types
// ============================================

export interface InputProps extends HTMLInputAttributes {
	value?: string;
	inputClass?: string;
}

// ============================================
// Paging Types
// ============================================

export interface PagingProps {
	currentPage?: number;
	totalPage?: number;
	pageSize?: number;
	pageSizeOptions?: number[];
	onPageChange?: (page: number) => void;
	onPageSizeChange?: (size: number) => void;
}

// ============================================
// Modal Types
// ============================================

export interface ModalProps {
	size?: Size;
	title?: string;
	children?: Snippet;
	footer?: Snippet;
	class?: string;
	dialogEl?: HTMLDialogElement;
}

// ============================================
// Table Types
// ============================================

export interface TableColumn<T = Record<string, unknown>> {
	label: string;
	key: keyof T & string;
}

export interface TableProps<T = Record<string, unknown>> {
	datas?: T[];
	columns?: TableColumn<T>[];
	showPaging?: boolean;
	paging?: PagingProps;
	trackBy?: keyof T;
	row?: Snippet<[{ data: T; columns: TableColumn<T>[]; rowIndex: number }]>;
	header?: Snippet<[{ columns: TableColumn<T>[] }]>;
	empty?: Snippet;
	onPageChange?: (page: number) => void;
}
