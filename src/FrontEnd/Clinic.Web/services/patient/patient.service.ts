import { HttpClient } from "@angular/common/http";
import { AppConfig } from "../app.config";
import { Pateint } from "../../models/patient/PatientModel";
import { firstValueFrom, Observable } from "rxjs";
import { Injectable } from "@angular/core";
import { Rdv } from "../../models/rdv/RdvModel";
import { VisitDocument } from "../../views/documents/document-info";

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

  public GetPatient(id: number): Promise<Pateint> {
    return firstValueFrom(this.http.get<Pateint>(`${this._apiUrl}/Patients/${id}`));
  }

  // Every document of every visit of the patient.
  public GetPatientDocuments(id: number): Promise<VisitDocument[]> {
    return firstValueFrom(this.http.get<VisitDocument[]>(`${this._apiUrl}/Patients/${id}/documents`));
  }

  // The patient file as a PDF: details, every visit, its notes and documents.
  public ExportPatientFile(id: number): Promise<Blob> {
    return firstValueFrom(this.http.get(`${this._apiUrl}/Patients/${id}/documents/export`, { responseType: 'blob' }));
  }

  public async DeletePatien(id:number)
  {
    return await this.http.delete(`${this._apiUrl}/Patients/${id}`).toPromise();
  }

}
