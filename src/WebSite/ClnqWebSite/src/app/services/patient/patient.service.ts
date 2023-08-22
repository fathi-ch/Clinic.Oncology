import { HttpClient } from "@angular/common/http";
import { AppConfig } from "../app.config";
import { Pateint } from "src/app/models/patient/PatientModel";
import { Observable } from "rxjs";
import { Injectable } from "@angular/core";

@Injectable()
export class ServicesPatient {

  public _apiUrl = AppConfig.settings.apiServer.clnqApi;
 
  constructor(private http: HttpClient) {
  }

  public GetAllPateints():Observable<Pateint[]>{
    return this.http.get<Pateint[]>(`${this._apiUrl}/Patients`);
  }

}
