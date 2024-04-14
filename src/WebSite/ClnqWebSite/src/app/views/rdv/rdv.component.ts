import { Component, NgModule, OnInit, ViewChild } from "@angular/core";
import { BrowserModule } from "@angular/platform-browser";
import { DxBulletModule, DxDataGridComponent, DxDataGridModule, DxDateBoxComponent, DxSchedulerComponent, DxTemplateModule, DxTextBoxComponent } from "devextreme-angular";
import { DxSchedulerTypes } from "devextreme-angular/ui/scheduler";
import { LoadOptions } from "devextreme/data";
import CustomStore from "devextreme/data/custom_store";
import DataSource from "devextreme/data/data_source";
import { AppointmentUpdatedEvent } from "devextreme/ui/scheduler";
import { BehaviorSubject } from "rxjs";

import { Pateint } from "src/app/models/patient/PatientModel";
import { Rdv } from "src/app/models/rdv/RdvModel";
import { RdvType } from "src/app/models/rdv/RdvTypeModel";
import { ServiceCmnObject } from "src/app/services/ServiceCmnObject";
import { ServicesPatient } from "src/app/services/patient/patient.service";
import { ServicesRdv } from "src/app/services/rdv/rdv.service";

@Component({
  selector: 'app-patients',
  templateUrl: './rdv.component.html',
  styleUrls: ['./rdv.component.scss']

})


export class RdvComponent implements OnInit {

  @ViewChild("dateNais", { static: false }) dateNais!: DxDateBoxComponent;
  @ViewChild("dateRdv", { static: false }) dateRdv!: DxDateBoxComponent;
  @ViewChild("startRdv", { static: false }) startRdv!: DxDateBoxComponent;
  @ViewChild("endRdv", { static: false }) endRdv!: DxDateBoxComponent;
  @ViewChild("scheduler_ges", { static: false })  scheduler_ges!: DxSchedulerComponent;



  rdvData: DataSource;


  lastRowOpned: string = "";
  //dataSource:[];

  allRdvData:Rdv[]=[];
  dataSource= [
    {
    id: 1,
    startTime: new Date('2024-04-14T13:00:00.000'),
    endTime: new Date('2024-004-14T13:15:00.000'),
    title:'NECIB Farouk'
    
  },
  {
    id: 2,
    startTime: new Date('2023-09-05T14:00:00.000Z'),
    endTime: new Date('2023-09-05T14:30:00.000Z'),
    title:'NECIB Yanis'
    
  },
  {
    id: 3,
    startTime: new Date('2023-09-05T15:00:00.000Z'),
    endTime: new Date('2023-09-05T15:30:00.000Z'),
    title:'CHABAN Fathi'
    
  },
  {
    id: 4,
    startTime: new Date('2023-09-05T16:00:00.000Z'),
    endTime: new Date('2023-09-05T17:30:00.000Z'),
    title:'CHABAN Issam'
    
  },
  {
    id: 5,
    startTime: new Date('2023-09-05T18:00:00.000Z'),
    endTime: new Date('2023-09-05T18:30:00.000Z'),
    title:'NECIB Ramzi'
    
  }
  ];
  //pop up new patient

  popupVisible = false;

  rdvType:RdvType[]=[
    {code:'RDV',type:'Rdv médical'},
    {code:'ABS',type:'Abcent'},]

  selectedType:string=""
  rdvPrix:number=0;
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
  min: Date = new Date(1900, 0, 1,7);
  max: Date = new Date(1900, 0, 1,22);
  dateClear = new Date(2015, 11, 1, 6);

  timeMin: Date = new Date(0, 0, 0, 7, 0, 0);
  timeMax: Date = new Date(0, 0, 0, 22, 0, 0);

  public currentDate: Date = new Date();

  searchText:string="";

  newRdvSelectedPatien:Pateint=new Pateint();


  // data grid data source 
  LstPatients: Pateint[] = [];

  constructor(
    public readonly PateintService: ServicesPatient,
    public readonly RdvService: ServicesRdv,
    private readonly serviceCmnObject: ServiceCmnObject
  ) {
    const that = this;
    this.rdvData=new DataSource({
     
      store: new CustomStore({
        key:'id',
       load:async (LoadOptions:LoadOptions)=>{
          let d=that.scheduler_ges.instance.getStartViewDate();
          let f=that.scheduler_ges.instance.getEndViewDate();

          // rendring all rdvs
          await that.RdvService.GetRdvByDate(d as Date,f as Date).then(data=>{
            that.allRdvData=  data as Rdv[];
         });

        return that.allRdvData;
          
        },
        update:(key,value)=>
        {
        
   
            
         this.updateRdv(value as Rdv)
          
          that.rdvData.reload();
          return  value;
        }
       
      }),
      paginate: false
    
  
  });

}
  
  
  async ngOnInit() {

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

  public searchPatient()
  {
    if(this.searchText && this.searchText.length>3)
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

  public newPatientPopUp() {
    this.popupVisible = true;
  }

  public getValueDisplay(data:any)
  {
    return data.firstName +" "+ data.lastNime
  }

  public newPatienPopUpInit() {
    this.pat_nom = "";
    this.pat_prenom = "";
    this.dateNais.value = "";
    this.dateRdv.value = "";
  }
  public selectedPatien(e:any)
  {
    if(e.selectedItem)
    {
        this.newRdvSelectedPatien=e.selectedItem;
    }
      
 
  }

  public onPopUpRdvOpen()
  {
    this.newRdvSelectedPatien=new Pateint();
    this.searchText="";
    this.dateRdv.instance.reset();
    this.startRdv.value=this.min;
    this.endRdv.value=this.min;
   
  }

  public async newRdv() 
  {
    // date début
      let startTime=new Date(this.dateRdv.value)
      startTime.setHours(new Date(this.startRdv.value).getHours());
      startTime.setMinutes(new Date(this.startRdv.value).getMinutes());
    // date fin       
      let endTime=new Date(this.dateRdv.value);
      endTime.setHours(new Date(this.endRdv.value).getHours());
      endTime.setMinutes(new Date(this.endRdv.value).getMinutes());
      let _patien=this.newRdvSelectedPatien;
      let _selectedType=this.selectedType;

    //   startTime = new Date(startTime);
    // var timeZoneDifference = (startTime.getTimezoneOffset() / 60) * -1; //convert to positive value.
    // startTime.setTime(startTime.getTime() + (timeZoneDifference * 60) * 60 * 1000);
    // startTime.toISOString();

    
    // endTime = new Date(endTime);
    // var timeZoneDifference = (endTime.getTimezoneOffset() / 60) * -1; //convert to positive value.
    // endTime.setTime(endTime.getTime() + (timeZoneDifference * 60) * 60 * 1000);
    // endTime.toISOString();


      let _rdv=new Rdv();
      _rdv.patientId=this.newRdvSelectedPatien.id;
      _rdv.startTime=startTime;
      _rdv.endTime=endTime;
      _rdv.price=this.rdvPrix;
      _rdv.status="PLA";
      _rdv.visitType=this.selectedType;
      _rdv.description="";

      await this.RdvService.NewRdv(_rdv);
      this.rdvData.reload();
      this.popupVisible=false;


  }

  public async updateRdv(rdv: Rdv) 
  {
   
    //   startTime = new Date(startTime);
    // var timeZoneDifference = (startTime.getTimezoneOffset() / 60) * -1; //convert to positive value.
    // startTime.setTime(startTime.getTime() + (timeZoneDifference * 60) * 60 * 1000);
    // startTime.toISOString();

    
    // endTime = new Date(endTime);
    // var timeZoneDifference = (endTime.getTimezoneOffset() / 60) * -1; //convert to positive value.
    // endTime.setTime(endTime.getTime() + (timeZoneDifference * 60) * 60 * 1000);
    // endTime.toISOString();
      await this.RdvService.UpdateRdv(rdv);
      this.rdvData.reload();


  }


  public async onAppointmentDeleting (e: DxSchedulerTypes.AppointmentDeletingEvent) {
     
    let rdv=e.appointmentData as Rdv;
    await this.RdvService.DeleteRdv(rdv.id as number);
    this.rdvData.reload();
    // Handler of the "appointmentDeleting" event
}

  public async getAllRdv()
  {
   
  }

  public onContentReady(e:any) {
    e.component.scrollTo(this.currentDate);
  }

  public onAppointmentUpdating(e:any) {
    let d=e;
  }

  public onAppointmentFormOpening(e:any)
  {
    e.cancel = true;
    this.popupVisible=true;
  }

  public  handlePropertyChange(e:any)
  {
    e.cancel = true;
   
  }

}

