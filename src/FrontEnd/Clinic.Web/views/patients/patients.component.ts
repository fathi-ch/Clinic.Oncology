import { Component, NgModule, OnInit, ViewChild } from "@angular/core";
import { BrowserModule } from "@angular/platform-browser";
import { DxBulletModule, DxDataGridComponent, DxDataGridModule, DxDateBoxComponent, DxTemplateModule, DxTextBoxComponent } from "devextreme-angular";

import { Pateint } from "../../models/patient/PatientModel";
import { Sexe } from "../../models/patient/SexeModel";
import { ServiceCmnObject } from "../../services/ServiceCmnObject"
import { ServicesPatient } from "../../services/patient/patient.service";
import { Router } from "@angular/router";

@Component({
    selector: 'app-patients',
    templateUrl: './patients.component.html',
    styleUrls: ['./patients.component.scss']

  })

  
  export class PatientsComponent implements OnInit {

    @ViewChild("dateNais", { static: false })dateNais!: DxDateBoxComponent;
    @ViewChild("dateRdv", { static: false }) dateRdv!: DxTextBoxComponent;
    @ViewChild("startRdv", { static: false })startRdv!: DxDateBoxComponent;
    @ViewChild("endRdv", { static: false }) endRdv!: DxTextBoxComponent;
    
    lastRowOpned:string="";
 

   //pop up new patient

   popupVisible=false;
   popupDetVisible=false;
   sexePteintList:Sexe[]=[];
   sexePteint:string="";
   pat_nom:string="";
   pat_prenom:string="";
   pat_mobile:string="";
   pat_email:string="";
   pat_ref:string="";
   pat_num_suc:string="";
   pat_date_n: Date = new Date();
   pat_date_rdv: Date = new Date();
   pat_date_rdv_start_Time: Date = new Date(1900, 0, 1);
   pat_date_rdv_End_Time: Date = new Date(1900, 0, 1);
   firstWorkDay2017: Date = new Date(2017, 0, 3);
   min: Date = new Date(1900, 0, 1,7);
   max: Date = new Date(1900, 0, 1,22);
   dateClear = new Date(2015, 11, 1, 6);

   timeMin: Date = new Date(0,0,0,7,0,0);
   timeMax: Date = new Date(0,0,0,22,0,0);

   searchText:string="";

   
    
    // data grid data source 
    LstPatients:Pateint[]=[];

    constructor(
        public readonly PateintService: ServicesPatient,
        private readonly serviceCmnObject:ServiceCmnObject,
        private readonly router: Router
      )
      {
        this.sexePteintList=[];
        let _sexeMale=new Sexe();
        _sexeMale.sexe="Male";
        this.sexePteintList.push(_sexeMale);
        let _sexeFemelle=new Sexe();
        _sexeFemelle.sexe="Femelle";
        this.sexePteintList.push(_sexeFemelle);
      }
    ngOnInit(): void {
 

      //this.GetAllPateintList();
    }

    public convertDate(date:string)
    {
        if(date)
        {
          let shortDate=date?.split('T')[0];
          return shortDate?.split('-')[2]+"-"+shortDate?.split('-')[1]+"-"+shortDate?.split('-')[0]
        }
        return "-";
    
    }

    public convertDateWithTime(date:string)
    {  if(date)
      {
        let shortDate=date.split('T')[0];
        return shortDate.split('-')[2]+"-"+shortDate.split('-')[1]+"-"+shortDate.split('-')[0]+ " à "+date.split('T')[1];
      }
      return "-";
    
    }

// go to datail 
public details(pateint:Pateint)
{
  this.serviceCmnObject.patientDetail.next(pateint);
  this.popupDetVisible=true;
  //  this.router.navigate(['/patientsDetail']);
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

    public searchPatient()
    {
      if(this.searchText && this.searchText.length>0)
      {
      //  this.serviceCmnObject.spinnerLoading.next(true);
        this.lastRowOpned = "";
        // calling all pateints end point 
        this.PateintService.SearchPateints(this.searchText).subscribe(lstPatients => {
          this.LstPatients = lstPatients;
        //  this.serviceCmnObject.spinnerLoading.next(false);
        });
      }
      else
      {
        this.LstPatients=[];
      }
   
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
      _patien.mobile=this.pat_mobile;
      _patien.email=this.pat_email;
      _patien.socialSecurityNumber=this.pat_num_suc;
      _patien.referral=this.pat_ref;
      _patien.gender=this.sexePteint;


      this.PateintService.NewPateint(_patien).subscribe(pat=>{
        this.GetAllPateintList();
        this.serviceCmnObject.spinnerLoading.next(true);
        this.popupVisible=false;
      });
      
      
      
      
    }

    public PopUpPatientDetailonShowing()
    {
      

    }
}

