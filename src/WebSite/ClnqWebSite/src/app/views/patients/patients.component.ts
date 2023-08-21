import { Component, NgModule, OnInit, ViewChild } from "@angular/core";
import { BrowserModule } from "@angular/platform-browser";
import { DxBulletModule, DxDataGridComponent, DxDataGridModule, DxTemplateModule } from "devextreme-angular";

import { Pateint } from "src/app/models/patient/PatientModel";
import { ServiceCmnObject } from "src/app/services/ServiceCmnObject";
import { ServicesPatient } from "src/app/services/patient/patient.service";

@Component({
    selector: 'app-patients',
    templateUrl: './patients.component.html',
    styleUrls: ['./patients.component.scss']

  })

  
  export class PatientsComponent implements OnInit {

   //pop up new patient
   popupVisible=false;

    lastRowOpned:string="";

    
    // data grid data source 
    LstPatients:Pateint[]=[];

    constructor(
        public readonly PateintService: ServicesPatient,
        private readonly serviceCmnObject:ServiceCmnObject
      )
      {

      }
    ngOnInit(): void {
      this.serviceCmnObject.spinnerLoading.next(true);
      this.lastRowOpned="";
       // calling all pateints end point 
        this.PateintService.GetAllPateints().subscribe(lstPatients=>{
         this.LstPatients= lstPatients;
         this.serviceCmnObject.spinnerLoading.next(false);
        });
    }

    public convertDate(date:string)
    {
      let shortDate=date.split('T')[0];

      return shortDate.split('-')[2]+"-"+shortDate.split('-')[1]+"-"+shortDate.split('-')[0]
    }

    public convertDateWithTime(date:string)
    {
      let shortDate=date.split('T')[0];

      return shortDate.split('-')[2]+"-"+shortDate.split('-')[1]+"-"+shortDate.split('-')[0]+ " à "+date.split('T')[1];
    }


// show patient details 
    public showRow(id:string)
    {
      
      if(this.lastRowOpned==id)
      {
        
        var lastElement =  <HTMLFormElement>document.getElementById(id);
        let disp=lastElement.style.display;
        lastElement.style.display= lastElement.style.display=='none'? 'contents':'none';
      }
      else
      {
        if(this.lastRowOpned && this.lastRowOpned.length>0)
        {
          var lastElement =  <HTMLFormElement>document.getElementById(this.lastRowOpned);
          lastElement.style.display='none';
          lastElement =  <HTMLFormElement>document.getElementById(id);
          lastElement.style.display=  'contents';
        }
        else
        {
         
            lastElement =  <HTMLFormElement>document.getElementById(id);
            lastElement.style.display='contents';
          
        }
  
      }
      this.lastRowOpned=id;
      
    }

    public newPatientPopUp()
    {
this.popupVisible=true;
    }
}

