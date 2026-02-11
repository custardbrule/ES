interface RouteConstants<K extends Record<string, string> = Record<string, string>> {
  base: string;
  toRoute: (params: K) => string;
}

type FormFieldRule<V = unknown> = {
  validate: (value: V) => boolean;
  message: string;
};