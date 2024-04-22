import { Component, EventEmitter, NgModule, OnInit, ViewChild } from "@angular/core";
import { BrowserModule } from "@angular/platform-browser";
import { DxBulletModule, DxDataGridComponent, DxDataGridModule, DxDateBoxComponent, DxSchedulerComponent, DxTemplateModule, DxTextBoxComponent, DxContextMenuComponent, DxAutocompleteComponent, DxNumberBoxComponent, DxTextAreaComponent } from "devextreme-angular";
import { DxSchedulerTypes } from "devextreme-angular/ui/scheduler";
import { LoadOptions } from "devextreme/data";
import CustomStore from "devextreme/data/custom_store";
import DataSource from "devextreme/data/data_source";
import { AppointmentUpdatedEvent } from "devextreme/ui/scheduler";
import { BehaviorSubject } from "rxjs";

import { Pateint } from "src/app/models/patient/PatientModel";
import { Rdv } from "src/app/models/rdv/RdvModel";
import { RdvType } from "src/app/models/rdv/RdvTypeModel";
import { PieceJointe } from "src/app/models/rdv/piecejointe";
import { ServiceCmnObject } from "src/app/services/ServiceCmnObject";
import { ServicesPatient } from "src/app/services/patient/patient.service";
import { ServicesRdv } from "src/app/services/rdv/rdv.service";

@Component({
  selector: 'app-rdvDetails',
  templateUrl: './rdvDetails.component.html',
  styleUrls: ['./rdvDetails.component.scss']

})


export class RdvDetailsComponent implements OnInit {

  @ViewChild("dateNais", { static: false }) dateNais!: DxDateBoxComponent;
  @ViewChild("dateRdv", { static: false }) dateRdv!: DxDateBoxComponent;
  @ViewChild("startRdv", { static: false }) startRdv!: DxDateBoxComponent;
  @ViewChild("endRdv", { static: false }) endRdv!: DxDateBoxComponent;
  @ViewChild("scheduler_ges", { static: false }) scheduler_ges!: DxSchedulerComponent;
  @ViewChild('contextMenu', { static: false }) contextMenu!: DxContextMenuComponent;
  @ViewChild('patientRdv', { static: false }) patientRdv!: DxAutocompleteComponent;
  @ViewChild('prixRdv', { static: false }) prixRdv!: DxNumberBoxComponent;
  @ViewChild('descText', { static: false }) descText!: DxTextAreaComponent;
  

  

  toAdd: boolean = true;
  rdvData: DataSource;
  onContextMenuItemClick: any;
  cellContextMenuItems: any[] | undefined;
  appointmentContextMenuItems: any[] | undefined;
  pjListeTemp:any[]=[];
  viewpj:string="";
  public pjListe : BehaviorSubject<PieceJointe[]>=new BehaviorSubject<PieceJointe[]>([]);

  lastRowOpned: string = "";
  //dataSource:[];

  allRdvData: Rdv[] = [];
  //pop up new patient

  popupVisible = false;

  rdvType: RdvType[] = [
    { code: 'RDV', type: 'Rdv médical' },
    { code: 'ABS', type: 'Abcent' },]

  selectedType: string = ""
  rdvPrix: number = 0;
  pat_nom: string = "";
  pat_prenom: string = "";
  pat_mobile: string = "";
  pat_email: string = "";
  pat_date_n: Date = new Date();
  pat_date_rdv: Date = new Date();
  pat_date_rdv_a: Date = new Date();
  pat_date_rdv_start_Time: Date = new Date(1900, 0, 1);
  pat_date_rdv_End_Time: Date = new Date(1900, 0, 1);
  firstWorkDay2017: Date = new Date(2017, 0, 3);
  min: Date = new Date(1900, 0, 1, 7);
  max: Date = new Date(1900, 0, 1, 22);
  dateClear = new Date(2015, 11, 1, 6);

  timeMin: Date = new Date(0, 0, 0, 7, 0, 0);
  timeMax: Date = new Date(0, 0, 0, 22, 0, 0);
  public currentRdv = new Rdv();

  public currentDate: Date = new Date();

  searchText: string = "";

  newRdvSelectedPatien: Pateint = new Pateint();


  // data grid data source 
  LstPatients: Pateint[] = [];
  pjList = new EventEmitter<PieceJointe>()

  constructor(
    public readonly PateintService: ServicesPatient,
    public readonly RdvService: ServicesRdv,
    private readonly serviceCmnObject: ServiceCmnObject
  ) {
    this.currentRdv={id:0,patient:{age:"0",firstName:"",lastName:"",birthDate:new Date(Date.now())},startTime:new Date(Date.now())};
    const that = this;
    this.rdvData = new DataSource({

      store: new CustomStore({
        key: 'id',
        load: async (LoadOptions: LoadOptions) => {
          let d = that.scheduler_ges.instance.getStartViewDate();
          let f = that.scheduler_ges.instance.getEndViewDate();

          // rendring all rdvs
          await that.RdvService.GetRdvByDate(d as Date, f as Date).then(data => {
            that.allRdvData = data as Rdv[];
          });

          return that.allRdvData;

        },
        update: (key, value) => {



          this.updateRdv(value as Rdv)

          that.rdvData.reload();
          return value;
        }

      }),
      paginate: false


    });

  }


  async ngOnInit() {
this.serviceCmnObject.rdvDetail.subscribe(rdv=>{
  this.currentRdv=rdv;
  if(rdv.description)
    this.descText.value=rdv.description;

})
   await this.refreshPj();
  }

  async refreshPj()
  {
    if(this.currentRdv.id)
      {
        await this.RdvService.GetDocument(this.currentRdv?.id).then(pj=>{
          this.pjListeTemp=pj;
              });
      }
  
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

  public RdvRefresh() {
    this.rdvData.reload();
  }

  public searchPatient() {
    if (this.searchText && this.searchText.length > 3) {
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
    this.toAdd = true;
    this.dateRdv.instance.reset();
    this.startRdv.value = this.min;
    this.endRdv.value = this.min;
    this.popupVisible = true;
    this.patientRdv.value = "";
    this.newRdvSelectedPatien = new Pateint();
    this.prixRdv.value=0;
  }

  public getValueDisplay(data: any) {
    return data.firstName + " " + data.lastNime
  }

  public newPatienPopUpInit() {
    this.pat_nom = "";
    this.pat_prenom = "";
    this.dateNais.value = "";
    this.dateRdv.value = "";
  }
  public selectedPatien(e: any) {
    if (e.selectedItem) {
      this.newRdvSelectedPatien = e.selectedItem;
    }


  }


  public onPopUpRdvOpen() {
    if (!this.currentRdv) {
      this.currentRdv = new Rdv();
      this.newRdvSelectedPatien = new Pateint();
      this.patientRdv.value = "";
      this.searchText = "";
      this.dateRdv.instance.reset();
      this.startRdv.value = this.min;
      this.endRdv.value = this.min;
      this.prixRdv.value=0;
    }


  }

  public async newRdv() {
    // date début
    let startTime = new Date(this.dateRdv.value)
    startTime.setHours(new Date(this.startRdv.value).getHours());
    startTime.setMinutes(new Date(this.startRdv.value).getMinutes());
    // date fin       
    let endTime = new Date(this.dateRdv.value);
    endTime.setHours(new Date(this.endRdv.value).getHours());
    endTime.setMinutes(new Date(this.endRdv.value).getMinutes());
    let _patien = this.newRdvSelectedPatien;
    let _selectedType = this.selectedType;

    let _rdv = new Rdv();
    _rdv.patientId = this.newRdvSelectedPatien.id;
    _rdv.startTime = startTime;
    _rdv.endTime = endTime;
    _rdv.price = this.rdvPrix;
    _rdv.status = "PLA";
    _rdv.visitType = this.selectedType;
    _rdv.description = "";

    await this.RdvService.NewRdv(_rdv);
    this.rdvData.reload();
    this.popupVisible = false;


  }

  public async updateRdvForm() {
    let _rdv = this.currentRdv;
    // date début
    let startTime = new Date(this.dateRdv.value)
    startTime.setHours(new Date(this.startRdv.value).getHours());
    startTime.setMinutes(new Date(this.startRdv.value).getMinutes());
    // date fin       
    let endTime = new Date(this.dateRdv.value);
    endTime.setHours(new Date(this.endRdv.value).getHours());
    endTime.setMinutes(new Date(this.endRdv.value).getMinutes());
    let _patien = this.newRdvSelectedPatien;
    let _selectedType = this.selectedType;


    _rdv.patientId = this.newRdvSelectedPatien.id;
    _rdv.startTime = startTime;
    _rdv.endTime = endTime;
    _rdv.price = this.rdvPrix;
    _rdv.status = "PLA";
    _rdv.visitType = this.selectedType;
    _rdv.description = "";

    await this.RdvService.UpdateRdv(_rdv);
    this.rdvData.reload();
    this.popupVisible = false;


  }

  public async updateRdv(rdv: Rdv) {


    await this.RdvService.UpdateRdv(rdv);
    this.rdvData.reload();


  }

  public async updateDetailRdv() {

this.currentRdv.description=this.descText.value;
    await this.RdvService.UpdateRdv(this.currentRdv);
   


  }


  public async onAppointmentDeleting(e: DxSchedulerTypes.AppointmentDeletingEvent) {

    let rdv = e.appointmentData as Rdv;
    await this.RdvService.DeleteRdv(rdv.id as number);
    this.rdvData.reload();
  }

  public async deleteRdv() {
    console.log(this.currentRdv);
    await this.RdvService.DeleteRdv(this.currentRdv.id as number);
    this.popupVisible = false;
    this.rdvData.reload();
  }

  public async getAllRdv() {

  }

  public onContentReady(e: any) {
    e.component.scrollTo(this.currentDate);
  }

  public onAppointmentClick(e: any) {
    e.cancel = true;
    this.onAppointmentFormOpening(e);

  }

  public onAppointmentUpdating(e: any) {
    // e.cancel = true;
  }

  public onAppointmentFormOpening(e: any) {
    this.toAdd = true;
    this.dateRdv.instance.reset();
    this.startRdv.value = this.min;
    this.endRdv.value = this.min;
    this.popupVisible = true;
    this.patientRdv.value = "";
    this.newRdvSelectedPatien = new Pateint();
    this.prixRdv.value=0;
  

    e.cancel = true;
    let rdv = e.appointmentData as Rdv;
    this.currentRdv = rdv;
    this.newRdvSelectedPatien = new Pateint();
    if (rdv.id) {
      if (rdv.patient) {
        this.newRdvSelectedPatien = rdv.patient;
        this.patientRdv.value = rdv.patient.autocpmliteValue ? rdv.patient.autocpmliteValue : "";
      }

      if (rdv.startTime) {

        this.dateRdv.value = new Date(rdv.startTime);
        this.startRdv.value = new Date(rdv.startTime);
      }

      if (rdv.endTime)
        this.endRdv.value = new Date(rdv.endTime);

      if (rdv.startTime) {

        this.dateRdv.value = new Date(rdv.startTime);
        this.startRdv.value = new Date(rdv.startTime);

      }
      if (rdv.endTime) {
        this.endRdv.value = new Date(rdv.endTime);
      }


      if (rdv.price)
        this.prixRdv.value = rdv.price;

      this.toAdd = false;
      
    }
    else {
      if (rdv.startTime) {

        this.dateRdv.value = new Date(rdv.startTime);
        this.startRdv.value = new Date(rdv.startTime);
      }

      if (rdv.endTime)
        this.endRdv.value = new Date(rdv.endTime);

      if (rdv.startTime) {

        this.dateRdv.value = new Date(rdv.startTime);
        this.startRdv.value = new Date(rdv.startTime);

      }
      if (rdv.endTime) {
        this.endRdv.value = new Date(rdv.endTime);
      }

      this.prixRdv.value =0;
      this.toAdd = true;
    }
    this.popupVisible = true;
  }

  public handlePropertyChange(e: any) {
    e.cancel = true;

  }
async DeletePj(id:number)
{
  await this.RdvService.DeleteDocument(id);
  await this.refreshPj();
}
  public filesUploaded(e:any)
  {
    if (e.value.length) {
      e.value.forEach((file: any) => {
       
         
       
          let reader = new FileReader();
          reader.onload = (fr: any) => {
            let img = new Image();
            img.src = reader.result?reader.result.toString():"";
            setTimeout(async () => {
              let currPJComment: PieceJointe = {
                visitId: 1,
                fileName: encodeURI(file.name.toString()),
                file64:img.src
              };
              
              this.pjList.emit(currPJComment);

              await this.RdvService.AddDocument(currPJComment);
              await this.refreshPj();


            }, 0);
          };
          //this.pjListe.next(this.pjListeTemp);
          this.pjListe.subscribe(v=>{
            this.pjListeTemp=v;
          })
          reader.readAsDataURL(file);
        
      
      
      });
    
      
    
  }
  }

}

