type ValidationRule = {
	validate: (value: unknown) => boolean;
	message: string;
};

type ValidationResult<S> = {
	isValid: boolean;
	errors: { [K in keyof S]?: string[] };
};

type FieldChain<S, T extends keyof S> = {
	add: (validate: (value: S[T]) => boolean, message: string) => FieldChain<S, T>;
	for: <K extends keyof S>(field: K) => FieldChain<S, K>;
};

class ValidatorBuilder<S> {
	private rules = new Map<keyof S, ValidationRule[]>();

	static create<T>(...configs: Array<(builder: ValidatorBuilder<T>) => void>): ValidatorBuilder<T> {
		const builder = new ValidatorBuilder<T>();
		configs.forEach((config) => config(builder));
		return builder;
	}

	for<T extends keyof S>(field: T): FieldChain<S, T> {
		if (!this.rules.has(field)) {
			this.rules.set(field, []);
		}
		const fieldRules = this.rules.get(field)!;

		const chain: FieldChain<S, T> = {
			add: (validate, message) => {
				fieldRules.push({ validate: validate as (value: unknown) => boolean, message });
				return chain;
			},
			for: <K extends keyof S>(f: K) => this.for(f)
		};

		return chain;
	}

	validate(data: S): ValidationResult<S> {
		const errors: { [K in keyof S]?: string[] } = {};
		let isValid = true;

		for (const [field, rules] of this.rules) {
			for (const rule of rules) {
				if (!rule.validate(data[field])) {
					isValid = false;
					(errors[field] ??= []).push(rule.message);
				}
			}
		}

		return { isValid, errors };
	}

	isFieldError(field: keyof S, result: ValidationResult<S>): boolean {
		return !!result.errors[field]?.length;
	}

	getFieldErrors(field: keyof S, result: ValidationResult<S>): string[] {
		return result.errors[field] || [];
	}
}

export { ValidatorBuilder, type ValidationResult };
