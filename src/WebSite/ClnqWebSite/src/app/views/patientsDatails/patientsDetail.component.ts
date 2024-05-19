import { Component, NgModule, OnInit, ViewChild } from "@angular/core";
import { BrowserModule } from "@angular/platform-browser";
import { DxBulletModule, DxDataGridComponent, DxDataGridModule, DxDateBoxComponent, DxFormComponent, DxTemplateModule, DxTextBoxComponent } from "devextreme-angular";

import { Pateint } from "../../models/patient/PatientModel";
import { Sexe } from "../../models/patient/SexeModel";
import { ServiceCmnObject } from "../../services/ServiceCmnObject";
import { ServicesPatient } from "../../services/patient/patient.service";
import { Rdv } from "../../models/rdv/RdvModel";
import { ServicesRdv } from "../../services/rdv/rdv.service";
import { HttpClient, HttpHandler } from "@angular/common/http";
import notify from 'devextreme/ui/notify';
import { Router } from "@angular/router";

@Component({
  selector: 'app-patientsDetail',
  templateUrl: './patientsDetail.component.html',
  styleUrls: ['./patientsDetail.component.scss']

})


export class patientsDetailComponent implements OnInit {

  @ViewChild("dateNais", { static: false }) dateNais!: DxDateBoxComponent;
  @ViewChild("dateRdv", { static: false }) dateRdv!: DxTextBoxComponent;
  @ViewChild("startRdv", { static: false }) startRdv!: DxDateBoxComponent;
  @ViewChild("endRdv", { static: false }) endRdv!: DxTextBoxComponent;
  @ViewChild(DxFormComponent, { static: false }) myform!: DxFormComponent;
  currentPatient: Pateint = new Pateint();
  currentRdv: Rdv = new Rdv();
  passedRdv:Rdv[]=[];
  allRdv: Rdv[] = [];
  itemData: string[] = [];
  lastRowOpned: string = "";
  positionEditorOptions: Object;
  visistListeEditorOptions: Object;
  textBoxOptions: Object;
  nextAppointment: Object;
  textBoxAeraOptions: Object
  positions: string[] = [
    'Male',
    'Femelle'
  ];
  pjListeTemp: any[] = [];
  //pop up new patient

  popupVisible = false;
  popupRdvDetVisible=false;
  editRdv=false;
  sexePteintList: Sexe[] = [];
  sexePteint: string = "";
  pat_nom: string = "";
  pat_prenom: string = "";
  pat_mobile: string = "";
  pat_email: string = "";
  pat_ref: string = "";
  pat_num_suc: string = "";
  pat_date_n: Date = new Date();
  pat_date_rdv: Date = new Date();
  pat_date_rdv_start_Time: Date = new Date(1900, 0, 1);
  pat_date_rdv_End_Time: Date = new Date(1900, 0, 1);
  firstWorkDay2017: Date = new Date(2017, 0, 3);
  min: Date = new Date(1900, 0, 1, 7);
  max: Date = new Date(1900, 0, 1, 22);
  dateClear = new Date(2015, 11, 1, 6);

  timeMin: Date = new Date(0, 0, 0, 7, 0, 0);
  timeMax: Date = new Date(0, 0, 0, 22, 0, 0);

  searchText: string = "";
  currentImageView: string = "";



  // data grid data source 
  LstPatients: Pateint[] = [];

  constructor(
    public readonly PateintService: ServicesPatient,
    public  RdvService: ServicesRdv,
    private readonly serviceCmnObject: ServiceCmnObject,
    private readonly router: Router
  ) {
    this.serviceCmnObject.spinnerLoading.next(true);
    this.serviceCmnObject.popupPatientDetailIsOpen.subscribe(open=>{
      this.editRdv=open;
    })
    this.serviceCmnObject.patientDetail.subscribe(async pateint => {
      this.currentRdv = new Rdv();
      this.allRdv = [];
      this.currentPatient = pateint;
      this.pjListeTemp = [];
      await this.getVisitsByPatient();
      this.serviceCmnObject.spinnerLoading.next(false);

    });

    this.positionEditorOptions = { items: this.positions, searchEnable: true, value: this.currentPatient.gender, width: 200 }
    this.visistListeEditorOptions = { items: "", searchEnable: true, value: this.currentRdv.visitDateTitel }
    this.nextAppointment = { disabled: true, width: 200 };
    this.textBoxOptions = { width: 200 };
    this.textBoxAeraOptions = { width: 700, height: 260, readOnly:true };
  }

  ngAfterViewInit() {
    this.myform.instance.validate();
  }
  ngOnInit(): void {

    
  }



  public viewImage(id: any) {
    let image = this.pjListeTemp.find(x => x.id == id);
    this.currentImageView = image.patientDocumentsbase64;
    this.popupVisible = true;
  }

  public async getVisitsByPatient() {
    this.serviceCmnObject.spinnerLoading.next(true)
    this.visistListeEditorOptions = { items: "", searchEnable: true, value: "", width: 200 }
    this.PateintService.GetAllVisitesByPateint(this.currentPatient.id as number).subscribe(async rdv => {
      this.allRdv = rdv;
      this.passedRdv=[];
      this.itemData =[];
      if (this.allRdv.length > 0) {
        this.passedRdv = this.allRdv.filter(rdv => rdv.status == 'TRM');
        
        if (this.passedRdv.length > 0) {

          this.passedRdv.forEach(item => {
            this.itemData.push(item.visitDateTitel as string);
          });
      
        }
      }
      this.serviceCmnObject.spinnerLoading.next(false);
    });
  }


public showRdvDetail()
{
  this.serviceCmnObject.rdvDetail.next(this.currentRdv);
  this.popupRdvDetVisible=true;

}
  public async onSelectionChanged(e:any)
  {
    
    this.currentRdv=e.selectedItem;
    this.pjListeTemp=[];
    this.editRdv=true;
    this.RdvService.GetDocument(this.currentRdv.id as number).then(pj => {
      this.pjListeTemp = pj;
    });
   
  }

  async refreshPj() {
    if (this.currentRdv.id) {
      await this.RdvService.GetDocument(this.currentRdv?.id).then(pj => {
        this.pjListeTemp = pj;
      });
    }

  }

  async DeletePj(id: number) {
    await this.RdvService.DeleteDocument(id);
    await this.refreshPj();
  }

  public convertDate(date: string) {
    let shortDate = date.split('T')[0];
    return shortDate.split('-')[2] + "-" + shortDate.split('-')[1] + "-" + shortDate.split('-')[0]
  }

  public convertDateWithTime(date: string) {
    let shortDate = date.split('T')[0];
    return shortDate.split('-')[2] + "-" + shortDate.split('-')[1] + "-" + shortDate.split('-')[0] + " à " + date.split('T')[1];
  }


  // show patient details 
  public showRow(id: string) {

    if (this.lastRowOpned == id) {

      var lastElement = <HTMLFormElement>document.getElementById(id);
      let disp = lastElement.style.display;
      lastElement.style.display = lastElement.style.display == 'none' ? 'contents' : 'none';
    }
    else {
      if (this.lastRowOpned && this.lastRowOpned.length > 0) {
        var lastElement = <HTMLFormElement>document.getElementById(this.lastRowOpned);
        lastElement.style.display = 'none';
        lastElement = <HTMLFormElement>document.getElementById(id);
        lastElement.style.display = 'contents';
      }
      else {

        lastElement = <HTMLFormElement>document.getElementById(id);
        lastElement.style.display = 'contents';

      }

    }
    this.lastRowOpned = id;

  }
  // update patient
  public async SetPateint() {
    await this.PateintService.SetPatient(this.currentPatient).subscribe(async setResult => {
      this.currentPatient = setResult;
      if((this.currentRdv.id) as number >0)
        {
          await this.RdvService.UpdateRdv(this.currentRdv).then();
        }
          
       
          notify({ message: "la modification est pris en compte", width: 300, shading: false }, "success", 2000);
    });

  
   
  }
   
  public async DeletePatient()
  {
    await this.PateintService.DeletePatien(this.currentPatient.id as number);
    notify({ message: "Patient est supprimé", width: 300, shading: false }, "success", 2000);
    document.location.reload();
  }

  public GetAllPateintList() {
    this.refreshPj();
    //this.serviceCmnObject.spinnerLoading.next(true);
    this.currentPatient;
  
  }

  public searchPatient() {
    if (this.searchText && this.searchText.length > 0) {
      //  this.serviceCmnObject.spinnerLoading.next(true);
      this.lastRowOpned = "";
      // calling all pateints end point 
      this.PateintService.SearchPateints(this.searchText).subscribe(lstPatients => {
        this.LstPatients = lstPatients;
        //  this.serviceCmnObject.spinnerLoading.next(false);
      });
    }
    else {
      this.LstPatients = [];
    }

  }

  public newPatientPopUp() {
    this.popupVisible = true;
  }

  public newPatienPopUpInit() {
    this.pat_nom = "";
    this.pat_prenom = "";
    this.dateNais.value = "";
    this.dateRdv.value = "";
  }

  public newPatient() {
    this.serviceCmnObject.spinnerLoading.next(true);
    let _patien = new Pateint();
    _patien.firstName = this.pat_nom;
    _patien.lastName = this.pat_prenom;
    _patien.birthDate = new Date(this.dateNais.value);
    _patien.nextAppointment = new Date(this.dateRdv.value);
    _patien.mobile = this.pat_mobile;
    _patien.socialSecurityNumber = this.pat_num_suc;
    _patien.referral = this.pat_ref;
    _patien.gender = this.sexePteint;


    this.PateintService.NewPateint(_patien).subscribe(pat => {
      this.GetAllPateintList();
      this.serviceCmnObject.spinnerLoading.next(true);
      this.popupVisible = false;
    });




  }
}



