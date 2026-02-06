export type IModel<T, K extends keyof T = keyof T> = {
	[P in K]: T[K];
} & Record<K, T[K]>;

export interface ClientViewModel {
	id: string;
	clientId: string;
	displayName: string;
	clientType: 'public' | 'confidential';
	redirectUris: string[];
	postLogoutRedirectUris: string[];
	permissions: string[];
}

export interface CreateClientModel extends Omit<ClientViewModel, 'id' | 'clientId'> {}
