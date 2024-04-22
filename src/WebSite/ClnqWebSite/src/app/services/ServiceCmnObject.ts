import { Injectable } from "@angular/core";
import { BehaviorSubject } from "rxjs";
import { Rdv } from "../models/rdv/RdvModel";


@Injectable({
    providedIn: 'root'
  })
  export class ServiceCmnObject{
    public spinnerLoading: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
    public rdvDetail: BehaviorSubject<Rdv> = new BehaviorSubject<Rdv>(new Rdv());

  }