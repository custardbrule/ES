// ============================================
// String Validators
// ============================================

export const required = (value: unknown): boolean => {
	if (value === null || value === undefined) return false;
	if (typeof value === 'string') return value.trim().length > 0;
	if (Array.isArray(value)) return value.length > 0;
	return true;
};

export const minLength = (min: number) => (value: string): boolean => {
	return value.length >= min;
};

export const maxLength = (max: number) => (value: string): boolean => {
	return value.length <= max;
};

export const pattern = (regex: RegExp) => (value: string): boolean => {
	return regex.test(value);
};

// ============================================
// Format Validators
// ============================================

export const email = (value: string): boolean => {
	return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value);
};

export const url = (value: string): boolean => {
	try {
		new URL(value);
		return true;
	} catch {
		return false;
	}
};

export const alphanumeric = (value: string): boolean => {
	return /^[a-zA-Z0-9]+$/.test(value);
};

export const numeric = (value: string): boolean => {
	return /^\d+$/.test(value);
};

// ============================================
// Number Validators
// ============================================

export const min = (minVal: number) => (value: number): boolean => {
	return value >= minVal;
};

export const max = (maxVal: number) => (value: number): boolean => {
	return value <= maxVal;
};

export const between = (minVal: number, maxVal: number) => (value: number): boolean => {
	return value >= minVal && value <= maxVal;
};

// ============================================
// Array Validators
// ============================================

export const minItems = (min: number) => (value: unknown[]): boolean => {
	return value.length >= min;
};

export const maxItems = (max: number) => (value: unknown[]): boolean => {
	return value.length <= max;
};

export const unique = (value: unknown[]): boolean => {
	return new Set(value).size === value.length;
};

// ============================================
// Comparison Validators
// ============================================

export const equals = <T>(expected: T) => (value: T): boolean => {
	return value === expected;
};

export const oneOf = <T>(options: T[]) => (value: T): boolean => {
	return options.includes(value);
};
