import { Injectable } from '@angular/core';
import {
  FormGroup,
  FormControl,
  FormArray,
  AbstractControl,
  ValidationErrors,
} from '@angular/forms';
import type { ValidatorFn, AsyncValidatorFn } from '@angular/forms';
import { from, map, type Observable } from 'rxjs';

type ElementOf<T> = T extends (infer E)[] ? E : never;

type ToFormControls<T, K extends keyof T = keyof T> = {
  [F in K]: AbstractControl<T[F]>;
};

interface ChainOptions {
  validators?: ValidatorFn[];
  asyncValidators?: AsyncValidatorFn[];
  updateOn?: 'change' | 'blur' | 'submit';
  disabled?: boolean;
}

type FieldEntry<T, K extends keyof T> =
  | { type: 'control'; defaultValue: T[K]; chain: ValidatorChain<T[K]> }
  | {
      type: 'group';
      builder: FormGroupBuilder<T[K]>;
      chain: ValidatorChain<T[K]>;
    }
  | {
      type: 'array';
      controls: FormControl[];
      chain: ValidatorChain<T[K]>;
    }
  | {
      type: 'arrayGroup';
      builders: FormGroupBuilder<ElementOf<T[K]>>[];
      chain: ValidatorChain<T[K]>;
    };

class ValidatorChain<V> {
  private validators: ValidatorFn[] = [];
  private asyncValidators: AsyncValidatorFn[] = [];
  private isDisabled = false;
  private updateOnStrategy: 'change' | 'blur' | 'submit' | null = null;

  add(validate: (value: V) => boolean, message: string): this {
    this.validators.push(
      (control: AbstractControl): ValidationErrors | null => {
        return validate(control.value) ? null : { [message]: true };
      },
    );
    return this;
  }

  addAsync(
    validate: (value: V) => Promise<boolean> | Observable<boolean>,
    message: string,
  ): this {
    this.asyncValidators.push(
      (control: AbstractControl): Observable<ValidationErrors | null> => {
        return from(validate(control.value)).pipe(
          map((valid: boolean) => (valid ? null : { [message]: true })),
        );
      },
    );
    return this;
  }

  disabled(value = true): this {
    this.isDisabled = value;
    return this;
  }

  updateOn(strategy: 'change' | 'blur' | 'submit'): this {
    this.updateOnStrategy = strategy;
    return this;
  }

  toOptions(): ChainOptions {
    return {
      validators: this.validators,
      asyncValidators: this.asyncValidators,
      updateOn: this.updateOnStrategy ?? undefined,
      disabled: this.isDisabled,
    };
  }
}

class FormGroupBuilder<T, K extends keyof T = keyof T> {
  private fields = new Map<K, FieldEntry<T, K>>();

  for<F extends K>(field: F, defaultValue: T[F]): ValidatorChain<T[F]> {
    const chain = new ValidatorChain<T[F]>();
    this.fields.set(field, {
      type: 'control',
      defaultValue: defaultValue,
      chain: chain,
    });
    return chain;
  }

  forGroup<F extends K>(
    field: F,
    config: (builder: FormGroupBuilder<T[F]>) => void,
  ): ValidatorChain<T[F]> {
    const chain = new ValidatorChain<T[F]>();
    const nested = new FormGroupBuilder<T[F]>();
    config(nested);
    this.fields.set(field, {
      type: 'group',
      builder: nested,
      chain: chain,
    } as unknown as FieldEntry<T, K>);
    return chain;
  }

  forArray<F extends K>(
    field: F,
    defaultValues: T[F] & unknown[],
    itemConfig?: (chain: ValidatorChain<ElementOf<T[F]>>) => void,
  ): ValidatorChain<T[F]> {
    const chain = new ValidatorChain<T[F]>();
    let itemValidators: ValidatorFn[] = [];
    let itemAsyncValidators: AsyncValidatorFn[] = [];

    if (itemConfig) {
      const itemChain = new ValidatorChain<ElementOf<T[F]>>();
      itemConfig(itemChain);
      const opts = itemChain.toOptions();
      itemValidators = opts.validators ?? [];
      itemAsyncValidators = opts.asyncValidators ?? [];
    }

    const controls = defaultValues.map(
      (v) => new FormControl(v, itemValidators, itemAsyncValidators),
    );
    this.fields.set(field, {
      type: 'array',
      controls: controls,
      chain: chain,
    });
    return chain;
  }

  forArrayGroup<F extends K>(
    field: F,
    items: ((builder: FormGroupBuilder<ElementOf<T[F]>>) => void)[],
  ): ValidatorChain<T[F]> {
    const chain = new ValidatorChain<T[F]>();
    const builders = items.map((config) => {
      const nested = new FormGroupBuilder<ElementOf<T[F]>>();
      config(nested);
      return nested;
    });
    this.fields.set(field, {
      type: 'arrayGroup',
      builders: builders,
      chain: chain,
    } as unknown as FieldEntry<T, K>);
    return chain;
  }

  private _build(
    parentOptions?: ChainOptions,
  ): FormGroup<ToFormControls<T, K>> {
    const controls = [...this.fields].reduce(
      (acc, [field, entry]) => {
        const opts = entry.chain.toOptions();

        switch (entry.type) {
          case 'control':
            acc[field as string] = new FormControl(
              { value: entry.defaultValue, disabled: !!opts.disabled },
              {
                validators: opts.validators?.length ? opts.validators : null,
                asyncValidators: opts.asyncValidators?.length
                  ? opts.asyncValidators
                  : null,
                ...(opts.updateOn && { updateOn: opts.updateOn }),
              },
            );
            break;

          case 'group': {
            const group = entry.builder._build(opts);
            if (opts.disabled) group.disable();
            acc[field as string] = group;
            break;
          }

          case 'array': {
            const array = new FormArray(entry.controls, {
              validators: opts.validators?.length ? opts.validators : null,
              asyncValidators: opts.asyncValidators?.length
                ? opts.asyncValidators
                : null,
              ...(opts.updateOn && { updateOn: opts.updateOn }),
            });
            if (opts.disabled) array.disable();
            acc[field as string] = array;
            break;
          }

          case 'arrayGroup': {
            const groups = entry.builders.map((b) => b.build());
            const array = new FormArray(groups, {
              validators: opts.validators?.length ? opts.validators : null,
              asyncValidators: opts.asyncValidators?.length
                ? opts.asyncValidators
                : null,
              ...(opts.updateOn && { updateOn: opts.updateOn }),
            });
            if (opts.disabled) array.disable();
            acc[field as string] = array;
            break;
          }
        }
        return acc;
      },
      {} as Record<string, AbstractControl>,
    );

    const groupOpts = parentOptions ?? {};
    return new FormGroup(controls as ToFormControls<T, K>, {
      validators: groupOpts.validators?.length ? groupOpts.validators : null,
      asyncValidators: groupOpts.asyncValidators?.length
        ? groupOpts.asyncValidators
        : null,
      ...(groupOpts.updateOn && { updateOn: groupOpts.updateOn }),
    });
  }

  build(): FormGroup<ToFormControls<T, K>> {
    return this._build();
  }
}

@Injectable({
  providedIn: 'root',
})
export class FormBuilderService {
  create<T, K extends keyof T = keyof T>(
    config: (builder: FormGroupBuilder<T, K>) => void,
  ): FormGroup<ToFormControls<T, K>> {
    const builder = new FormGroupBuilder<T, K>();
    config(builder);
    return builder.build();
  }
}
