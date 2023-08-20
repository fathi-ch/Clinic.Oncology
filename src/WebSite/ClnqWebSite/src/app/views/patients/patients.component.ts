import { Component, NgModule, OnInit, ViewChild } from "@angular/core";
import { BrowserModule } from "@angular/platform-browser";
import { DxBulletModule, DxDataGridComponent, DxDataGridModule, DxTemplateModule } from "devextreme-angular";

import { Pateint } from "src/app/models/patient/PatientModel";
import { ServicesPatient } from "src/app/services/patient/patient.service";

@Component({
    selector: 'app-patients',
    templateUrl: './patients.component.html',
    styleUrls: ['./patients.component.scss']

  })

  
  export class PatientsComponent implements OnInit {

    // data grid pager
    showPageSizeSelector = true;
    showInfo = true;
    showNavButtons = true;  
    showNavigationButtons= true;  
    // data grid data source 
    LstPatients:Pateint[]=[];

    constructor(
        public readonly PateintService: ServicesPatient,
      )
      {

      }
    ngOnInit(): void {

       // calling all pateints end point 
        this.PateintService.GetAllPateints().subscribe(lstPatients=>{
         this.LstPatients= lstPatients;
        });
    }

    public convertDate(date:string)
    {
      let shortDate=date.split('T')[0];

      return shortDate.split('-')[2]+"-"+shortDate.split('-')[1]+"-"+shortDate.split('-')[0]
    }
}

