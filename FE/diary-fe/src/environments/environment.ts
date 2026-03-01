export const environment = {
  production: false,
  diaryApiUrl: 'http://localhost:5200',
  oidc: {
    issuer: 'http://localhost:5100/',
    clientId: 'diary-client',
    scope: 'openid profile',
  },
};
