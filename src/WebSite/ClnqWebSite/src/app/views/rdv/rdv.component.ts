import { Component, NgModule, OnInit, ViewChild } from "@angular/core";
import { BrowserModule } from "@angular/platform-browser";
import { DxBulletModule, DxDataGridComponent, DxDataGridModule, DxDateBoxComponent, DxSchedulerComponent, DxTemplateModule, DxTextBoxComponent } from "devextreme-angular";
import { LoadOptions } from "devextreme/data";
import CustomStore from "devextreme/data/custom_store";
import DataSource from "devextreme/data/data_source";
import { AppointmentUpdatedEvent } from "devextreme/ui/scheduler";

import { Pateint } from "src/app/models/patient/PatientModel";
import { Rdv } from "src/app/models/rdv/RdvModel";
import { ServiceCmnObject } from "src/app/services/ServiceCmnObject";
import { ServicesPatient } from "src/app/services/patient/patient.service";

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

dataSource= [
  {
  id: 1,
  startTime: new Date('2023-08-24T13:00:00.000'),
  endTime: new Date('2023-08-24T13:15:00.000'),
  title:'NECIB Farouk'
  
},
{
  id: 2,
  startTime: new Date('2023-08-24T14:00:00.000Z'),
  endTime: new Date('2023-08-24T14:30:00.000Z'),
  title:'NECIB Yanis'
  
},
{
  id: 3,
  startTime: new Date('2023-08-24T15:00:00.000Z'),
  endTime: new Date('2023-08-24T15:30:00.000Z'),
  title:'CHABAN Fathi'
  
},
{
  id: 4,
  startTime: new Date('2023-08-24T16:00:00.000Z'),
  endTime: new Date('2023-08-24T17:30:00.000Z'),
  title:'CHABAN Issam'
  
},
{
  id: 5,
  startTime: new Date('2023-08-24T18:00:00.000Z'),
  endTime: new Date('2023-08-24T18:30:00.000Z'),
  title:'NECIB Ramzi'
  
}
];
  //pop up new patient

  popupVisible = false;

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
  min: Date = new Date(1900, 0, 1);
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
    private readonly serviceCmnObject: ServiceCmnObject
  ) {
    const that = this;
    this.rdvData=new DataSource({
     
      store: new CustomStore({
        key:'id',
        load:function(LoadOptions:LoadOptions){
          let d=that.scheduler_ges.instance.getStartViewDate();
          let f=that.scheduler_ges.instance.getEndViewDate();
          return  that.dataSource;
        },
        update:(key,value)=>
        {
        
          
          that.dataSource.splice(5,1);
          that.dataSource.push(value);
          
          that.rdvData.reload();
          return  value;
        }
       
      }),
      paginate: false
    
  
  });

}
  
  
  ngOnInit(): void {
    
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

  public GetAllPateintList() {
    this.serviceCmnObject.spinnerLoading.next(true);
    this.lastRowOpned = "";
    // calling all pateints end point 
    this.PateintService.GetAllPateints().subscribe(lstPatients => {
      this.LstPatients = lstPatients;
      this.serviceCmnObject.spinnerLoading.next(false);
    });
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
  public newRdv() {
     
      let startTime=new Date(this.dateRdv.value);
      startTime.setHours(new Date(this.endRdv.value).getHours());
      startTime.setMinutes(new Date(this.endRdv.value).getMinutes());
   
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

