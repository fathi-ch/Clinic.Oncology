import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../environmont/environmont';
import { IAppConfig } from '../models/app-config.model';
import { Observable } from 'rxjs';
@Injectable()
export class AppConfig {
    static settings: IAppConfig;
    constructor(private http: HttpClient) {}
    load() {
        

        let env=environment.production;
       
        const jsonFile = env? 'assets/config/config.json':'assets/config/config.dev.json';
        return new Promise<void>((resolve, reject) => {
            this.http.get(jsonFile).toPromise()
            .then((response) => {
               AppConfig.settings = <IAppConfig>response;
               resolve();
            }).catch((response: any) => {
               reject(`Could not load file '${jsonFile}': ${JSON.stringify(response)}`);
            });
        });
    }

    loadObservable():Observable<IAppConfig>
    {
        let env=environment.production;
        const jsonFile = env? 'assets/config/config.json':'assets/config/config.dev.json';
        return this.http.get<IAppConfig>(jsonFile);
    }
}