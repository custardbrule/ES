import { Injectable } from '@angular/core';
import {
  FormGroup,
  FormControl,
  FormArray,
  AbstractControl,
  ValidationErrors,
} from '@angular/forms';
import type { ValidatorFn } from '@angular/forms';

type ElementOf<T> = T extends (infer E)[] ? E : never;

type ToFormControls<T, K extends keyof T = keyof T> = {
  [F in K]: AbstractControl<T[F]>;
};

type FieldEntry<T, K extends keyof T> =
  | { type: 'control'; defaultValue: T[K]; chain: FieldChain<T, K> }
  | { type: 'group'; group: FormGroup }
  | { type: 'array'; array: FormArray };

class FieldChain<T, K extends keyof T> {
  private validators: ValidatorFn[] = [];

  add(validate: (value: T[K]) => boolean, message: string): this {
    this.validators.push(
      (control: AbstractControl): ValidationErrors | null => {
        return validate(control.value) ? null : { [message]: true };
      },
    );
    return this;
  }

  _getValidators(): ValidatorFn[] {
    return this.validators;
  }
}

class FormGroupBuilder<T, K extends keyof T = keyof T> {
  private fields = new Map<K, FieldEntry<T, K>>();

  for<F extends K>(field: F, defaultValue: T[F]): FieldChain<T, F> {
    const chain = new FieldChain<T, F>();
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
  ): void {
    const nested = new FormGroupBuilder<T[F]>();
    config(nested);
    this.fields.set(field, {
      type: 'group',
      group: nested._build(),
    });
  }

  forArray<F extends K>(field: F, defaultValues: T[F] & unknown[]): void {
    const controls = defaultValues.map((v) => new FormControl(v));
    this.fields.set(field, { type: 'array', array: new FormArray(controls) });
  }

  forArrayGroup<F extends K>(
    field: F,
    items: ((builder: FormGroupBuilder<ElementOf<T[F]>>) => void)[],
  ): void {
    const groups = items.map((config) => {
      const nested = new FormGroupBuilder<ElementOf<T[F]>>();
      config(nested);
      return nested._build();
    });
    this.fields.set(field, { type: 'array', array: new FormArray(groups) });
  }

  _build(): FormGroup<ToFormControls<T, K>> {
    const controls = [...this.fields].reduce(
      (acc, [field, entry]) => {
        if (entry.type === 'control') {
          acc[field as string] = new FormControl(
            entry.defaultValue,
            entry.chain._getValidators(),
          );
        } else if (entry.type === 'group') {
          acc[field as string] = entry.group;
        } else {
          acc[field as string] = entry.array;
        }
        return acc;
      },
      {} as Record<string, AbstractControl>,
    );
    return new FormGroup(controls as ToFormControls<T, K>);
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
    return builder._build();
  }
}
