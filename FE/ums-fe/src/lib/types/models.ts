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

export interface ClientDetailsViewModel {
	id: string;
	clientId: string;
	displayName: string;
	clientType: 'public' | 'confidential';
	consentType: string;
	redirectUris: string[];
	postLogoutRedirectUris: string[];
	permissions: string[];
	roles: ClientRoleViewModel[];
	scopes: ClientScopeViewModel[];
}

export interface ClientScopeViewModel {
	id: string;
	applicationId: string;
	scopeId: string;
}

export interface ClientRoleViewModel {
	id: string;
	applicationId: string;
	name: string;
	description: string;
	scopes: string[];
}

export interface CreateClientRoleModel extends Omit<ClientRoleViewModel, 'id' | 'applicationId'> {}
export interface UpdateClientRoleModel extends Omit<ClientRoleViewModel, 'id' | 'applicationId'> {}

export interface CreateClientModel extends Omit<
	ClientViewModel,
	'id' | 'clientId' | 'permissions'
> {}

export interface UpdateClientModel extends Omit<
	ClientViewModel,
	'id' | 'clientId' | 'permissions'
> {}
