import type { Snippet } from 'svelte';
import type { HTMLButtonAttributes, HTMLInputAttributes } from 'svelte/elements';

// ============================================
// Shared Types
// ============================================

export type Size = 'sm' | 'md' | 'lg';

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
	onPageChange?: (page: number) => void;
}

// ============================================
// Modal Types
// ============================================

export interface ModalProps {
	open?: boolean;
	size?: Size;
	title?: string;
	children?: Snippet;
	footer?: Snippet;
	closeOnBackdrop?: boolean;
	onClose?: () => void;
	class?: string;
}

// ============================================
// Table Types
// ============================================

export interface TableColumn {
	label: string;
	key: string;
}

export interface TableProps {
	datas?: Record<string, unknown>[];
	columns?: TableColumn[];
	showPaging?: boolean;
	paging?: PagingProps;
	row?: Snippet<[{ data: Record<string, unknown>; columns: TableColumn[]; rowIndex: number }]>;
	header?: Snippet<[{ columns: TableColumn[] }]>;
	empty?: Snippet;
	onPageChange?: (page: number) => void;
}
