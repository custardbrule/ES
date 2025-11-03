type RouteBase = 'home' | 'collection';

type RouteParams = {
  home: Record<string, never>;
  collection: { id: string };
};

type RouteQuery = {
  home: { search?: string };
  collection: { filter?: string; sort?: 'asc' | 'desc' };
};

interface RouteConstants<K extends RouteBase> {
  base: string;
  toRoute: (params: RouteParams[K], query?: RouteQuery[K]) => string;
}

const buildQueryString = (query?: Record<string, any>): string => {
  if (!query || Object.keys(query).length === 0) return '';
  return '?' + new URLSearchParams(
    Object.fromEntries(
      Object.entries(query).filter(([_, v]) => v !== undefined)
    )
  ).toString();
};

export const ROUTE_DEF: {
  [K in RouteBase]: RouteConstants<K>
} = {
  home: {
    base: '',
    toRoute: (params, query) => buildQueryString(query),
  },
  collection: {
    base: 'collection/:id',
    toRoute: (params, query) => 
      `collection/${params.id}${buildQueryString(query)}`,
  },
} as const;