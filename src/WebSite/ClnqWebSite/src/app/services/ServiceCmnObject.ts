import { Injectable } from "@angular/core";
import { BehaviorSubject } from "rxjs";
import { Rdv } from "../models/rdv/RdvModel";
import { Pateint } from "../models/patient/PatientModel";
import { PieceJointe } from "../models/rdv/piecejointe";
import { ServicesRdv } from "./rdv/rdv.service";


@Injectable({
    providedIn: 'root'
  })
  export class ServiceCmnObject{
    public spinnerLoading: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
    public rdvDetail: BehaviorSubject<Rdv> = new BehaviorSubject<Rdv>(new Rdv());
    public patientDetail: BehaviorSubject<Pateint> = new BehaviorSubject<Pateint>(new Pateint());
    public listPj: BehaviorSubject<PieceJointe[]> = new BehaviorSubject<PieceJointe[]>([]);
    public popupPatientDetailIsOpen: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);




  
  
  }

