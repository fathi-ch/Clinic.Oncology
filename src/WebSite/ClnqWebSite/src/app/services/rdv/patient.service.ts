import { HttpClient } from "@angular/common/http";
import { AppConfig } from "../app.config";
import { Pateint } from "src/app/models/patient/PatientModel";
import { Observable } from "rxjs";
import { Injectable } from "@angular/core";

@Injectable()
export class ServicesRdv {

  public _apiUrl = AppConfig.settings.apiServer.clnqApi;
 
  constructor(private http: HttpClient) {
  }

  public GetAllPateints():Observable<Pateint[]>{
    return this.http.get<Pateint[]>(`${this._apiUrl}/Patients`);
  }

  public NewPateint(patient:Pateint): Observable<Pateint>{
    return this.http.post(`${this._apiUrl}/Patients`,patient);
     
  }

}
