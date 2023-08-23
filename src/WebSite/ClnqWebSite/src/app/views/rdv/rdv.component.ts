import { Component, NgModule, OnInit, ViewChild } from "@angular/core";
import { BrowserModule } from "@angular/platform-browser";
import { DxBulletModule, DxDataGridComponent, DxDataGridModule, DxDateBoxComponent, DxTemplateModule, DxTextBoxComponent } from "devextreme-angular";

import { Pateint } from "src/app/models/patient/PatientModel";
import { ServiceCmnObject } from "src/app/services/ServiceCmnObject";
import { ServicesPatient } from "src/app/services/patient/patient.service";

@Component({
    selector: 'app-patients',
    templateUrl: './rdv.component.html',
    styleUrls: ['./rdv.component.scss']

  })

  
  export class RdvComponent implements OnInit {

    @ViewChild("dateNais", { static: false })dateNais!: DxDateBoxComponent;
    @ViewChild("dateRdv", { static: false }) dateRdv!: DxTextBoxComponent;
    @ViewChild("startRdv", { static: false })startRdv!: DxDateBoxComponent;
    @ViewChild("endRdv", { static: false }) endRdv!: DxTextBoxComponent;
    
    lastRowOpned:string="";
 

   //pop up new patient

   popupVisible=false;

   pat_nom:string="";
   pat_prenom:string="";
   pat_mobile:string="";
   pat_email:string="";
   pat_date_n: Date = new Date();
   pat_date_rdv: Date = new Date();
   pat_date_rdv_start_Time: Date = new Date(1900, 0, 1);
   pat_date_rdv_End_Time: Date = new Date(1900, 0, 1);
   firstWorkDay2017: Date = new Date(2017, 0, 3);
   min: Date = new Date(1900, 0, 1);
   dateClear = new Date(2015, 11, 1, 6);

   timeMin: Date = new Date(0,0,0,7,0,0);
   timeMax: Date = new Date(0,0,0,22,0,0);

   
    
    // data grid data source 
    LstPatients:Pateint[]=[];

    constructor(
        public readonly PateintService: ServicesPatient,
        private readonly serviceCmnObject:ServiceCmnObject
      )
      {

      }
    ngOnInit(): void {
      this.GetAllPateintList();
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

    public GetAllPateintList()
    {
      this.serviceCmnObject.spinnerLoading.next(true);
      this.lastRowOpned="";
       // calling all pateints end point 
        this.PateintService.GetAllPateints().subscribe(lstPatients=>{
         this.LstPatients= lstPatients;
         this.serviceCmnObject.spinnerLoading.next(false);
        });
    }

    public newPatientPopUp()
    {
      this.popupVisible=true;
    }

    public newPatienPopUpInit()
    {
      this.pat_nom="";
      this.pat_prenom="";
      this.dateNais.value="";
      this.dateRdv.value="";
    }

    public newPatient()
    {
      this.serviceCmnObject.spinnerLoading.next(true);
      let _patien=new Pateint();
      _patien.firstName=this.pat_nom;
      _patien.lastName=this.pat_prenom;
      _patien.birthDate=new Date(this.dateNais.value);
      _patien.nextAppointment=new Date(this.dateRdv.value);

      this.PateintService.NewPateint(_patien).subscribe(pat=>{
        this.GetAllPateintList();
        this.serviceCmnObject.spinnerLoading.next(true);
        this.popupVisible=false;
      });
      
      
      
      
    }
}

