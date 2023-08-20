export interface IAppConfig {
    env: {
        name: string;
    };
    apiServer: {
        clnqApi: string;
    };
    logging: {
        console: boolean;
        appInsights: boolean;
    };
    auth: {
        requireAuth: boolean;
        tenant: string;
        clientId: string;
    };
  
}