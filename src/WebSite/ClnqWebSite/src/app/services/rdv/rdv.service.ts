import { HttpClient } from "@angular/common/http";
import { AppConfig } from "../app.config";
import { Pateint } from "src/app/models/patient/PatientModel";
import { Observable } from "rxjs";
import { Injectable } from "@angular/core";
import { Rdv } from "src/app/models/rdv/RdvModel";
import { PieceJointe } from "src/app/models/rdv/piecejointe";


@Injectable()
export class ServicesRdv {

  public _apiUrl = AppConfig.settings.apiServer.clnqApi;
 
  constructor(private http: HttpClient) {
  }

 

  public async GetAllRdv(){
    return await this.http.get<Rdv[]>(`${this._apiUrl}/visits`).toPromise();
  }

  public async GetRdvByDate(dateFrom:Date,toDate:Date){
    return await this.http.get<Rdv[]>(`${this._apiUrl}/visits/${dateFrom.toDateString()}/${toDate.toDateString()}`).toPromise();
  }

  public async GetRdvById(id:number){
    return await this.http.get<Rdv>(`${this._apiUrl}/visits/${id}`).toPromise();
  }

  public async DeleteRdv(id:number){
    return await this.http.delete(`${this._apiUrl}/visits/${id}`).toPromise();
  }

public async AddDocument(file:PieceJointe)
{
  return await this.http.post<any>(`${this._apiUrl}/documents`,file).toPromise();
 
}

public async GetDocument(visitId:number)
{
  return await this.http.get<any>(`${this._apiUrl}/visits/${visitId}/documents`).toPromise();
 
}
public async DeleteDocument(Id:number)
{
  return await this.http.delete(`${this._apiUrl}/documents/${Id}`).toPromise();
 
}
//   public GetRdv():Promise<Rdv[]>{

//     return this.http.get<Rdv[]>(`${this._apiUrl}/visits`);
//      this.http.get<Rdv[]>(`${this._apiUrl}/visits`).toPromise().then(data=>{
// return data;
//     });
//     return null;
//   }

  public async NewRdv(rdv:Rdv){
    return await this.http.post(`${this._apiUrl}/visits`,rdv).toPromise();
     
  }

  public async UpdateRdv(rdv:Rdv){
  
    
    return await this.http.put(`${this._apiUrl}/visits/${rdv.id}`,rdv).toPromise();
     
  }

}
