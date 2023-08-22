import { Injectable } from "@angular/core";
import { BehaviorSubject } from "rxjs";


@Injectable({
    providedIn: 'root'
  })
  export class ServiceCmnObject{
    public spinnerLoading: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
  }