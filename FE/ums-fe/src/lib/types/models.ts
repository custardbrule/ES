export interface CreateClientModel {
	displayName: string;
	clientType: 'public' | 'confidential';
	redirectUris: string[];
	postLogoutRedirectUris: string[];
	permissions: string[];
}
