import { HttpClient } from "@angular/common/http";
import { AppConfig } from "../app.config";
import { Pateint } from "../../models/patient/PatientModel";
import { Observable } from "rxjs";
import { Injectable } from "@angular/core";
import { Rdv } from "../../models/rdv/RdvModel";

@Injectable()
export class ServicesPatient {

  public _apiUrl = AppConfig.settings.apiServer.clnqApi;
 
  constructor(private http: HttpClient) {
  }

  public GetAllPateints():Observable<Pateint[]>{
    return this.http.get<Pateint[]>(`${this._apiUrl}/Patients`);
  }

  public GetAllVisitesByPateint(id:number):Observable<Rdv[]>{
    return this.http.get<Rdv[]>(`${this._apiUrl}/Patients/${id}/visits`);
  }

  public SearchPateints(name:string):Observable<Pateint[]>{
    return this.http.get<Pateint[]>(`${this._apiUrl}/Patients/SearchPatients?firstName=${name}`);
  }

  public NewPateint(patient:Pateint): Observable<Pateint>{
    return this.http.post(`${this._apiUrl}/Patients`,patient);
     
  }

  public SetPatient(patient:Pateint):Observable<Pateint>
  {
    return this.http.put<Pateint>(`${this._apiUrl}/Patients/${patient.id}`,patient);
  }

  public async DeletePatien(id:number)
  {
    return await this.http.delete(`${this._apiUrl}/Patients/${id}`).toPromise();
  }

}
