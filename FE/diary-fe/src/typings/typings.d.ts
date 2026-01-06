interface RouteConstants<K extends Record<string, string> = Record<string, string>> {
  base: string;
  toRoute: (params: K) => string;
}