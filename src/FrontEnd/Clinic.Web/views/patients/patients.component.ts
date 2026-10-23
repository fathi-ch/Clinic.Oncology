import { Component, NgModule, OnInit, ViewChild } from "@angular/core";
import { BrowserModule } from "@angular/platform-browser";
import { DxBulletModule, DxDataGridComponent, DxDataGridModule, DxDateBoxComponent, DxTemplateModule, DxTextBoxComponent } from "devextreme-angular";

import { Pateint } from "../../models/patient/PatientModel";
import { Sexe } from "../../models/patient/SexeModel";
import { ServiceCmnObject } from "../../services/ServiceCmnObject"
import { ServicesPatient } from "../../services/patient/patient.service";
import { Router } from "@angular/router";
import notify from "devextreme/ui/notify";
import { Rdv } from "../../models/rdv/RdvModel";
import { ServicesRdv } from "../../services/rdv/rdv.service";
import { BookingSlots } from "../rdv/booking-slots";

@Component({
    selector: 'app-patients',
    templateUrl: './patients.component.html',
    styleUrls: ['./patients.component.scss']

  })

  
  export class PatientsComponent implements OnInit {

    @ViewChild("dateNais", { static: false })dateNais!: DxDateBoxComponent;
    @ViewChild("dataGrid", { static: false }) dataGrid?: DxDataGridComponent;
    @ViewChild("dateRdv", { static: false }) dateRdv!: DxTextBoxComponent;
    // the "Prochain Rdv" de / à lists offer only free slots of the chosen day
    slots = new BookingSlots(day => this.RdvService.GetRdvByDate(day, day));
    
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
        private readonly router: Router,
        private readonly RdvService: ServicesRdv
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
 

      this.GetAllPateintList();
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
  this.router.navigate(['/patients', pateint.id]);
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

    // Search runs on the loaded list, across every column (nom, prénom, mobile...).
    public searchPatient()
    {
      this.dataGrid?.instance.searchByText(this.searchText ?? "");
    }

    // A click on a row opens or closes its detail; the name link and the expand arrow handle themselves.
    public toggleRow(e: any)
    {
      // the arrow cell already toggles the row itself
      if (e.rowType !== 'data' || e.event?.target?.closest?.('.dx-command-expand')) return;
      if (e.component.isRowExpanded(e.key)) e.component.collapseRow(e.key);
      else e.component.expandRow(e.key);
    }

    public goHome()
    {
      this.router.navigate(['/accueil']);
    }

    public newPatientPopUp()
    {
      this.slots.load(null, null, null);
      this.popupVisible=true;
    }

    public onDateRdvChanged(e: any)
    {
      const keep = (v: number | null) => (v === null ? null : new Date(v));
      this.slots.load(e.value ? new Date(e.value) : null, keep(this.slots.startValue), keep(this.slots.endValue));
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
      const booking = this.dateRdv.value ? this.slots.chosen() : null;
      if (this.dateRdv.value && !booking) {
        notify("Choisissez l'heure de début et de fin du rdv.", 'warning', 3000);
        return;
      }
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


      this.PateintService.NewPateint(_patien).subscribe(async pat=>{
        if (booking && pat.id) await this.bookFirstVisit(pat.id, booking);
        this.GetAllPateintList();
        this.popupVisible=false;
      });
    }

    // The "Prochain Rdv" fields book the patient's first visit once the patient exists.
    private async bookFirstVisit(patientId: number, booking: { start: Date; end: Date })
    {
      const rdv = new Rdv();
      rdv.patientId = patientId;
      rdv.startTime = booking.start;
      rdv.endTime = booking.end;
      rdv.price = 0;
      rdv.status = "PLA";
      rdv.visitType = "";
      rdv.description = "";
      rdv.weight = 0;
      rdv.height = 0;
      rdv.visitDateTitel = "";
      rdv.billingType = "PAID";
      try {
        await this.RdvService.NewRdv(rdv);
      } catch (err: any) {
        notify(err?.status === 409
          ? "Patient ajouté, mais ce créneau vient d'être réservé. Réservez le rdv depuis l'agenda."
          : "Patient ajouté, mais le rdv n'a pas pu être enregistré.", 'error', 5000);
      }
    }

    public PopUpPatientDetailonShowing()
    {
      

    }
}

