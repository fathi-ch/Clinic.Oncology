import { Component, EventEmitter, NgModule, OnInit, Output, ViewChild } from "@angular/core";
import { BrowserModule } from "@angular/platform-browser";
import { DxBulletModule, DxDataGridComponent, DxDataGridModule, DxDateBoxComponent, DxSchedulerComponent, DxTemplateModule, DxTextBoxComponent, DxContextMenuComponent, DxAutocompleteComponent, DxNumberBoxComponent, DxTextAreaComponent } from "devextreme-angular";
import { DxSchedulerTypes } from "devextreme-angular/ui/scheduler";
import { LoadOptions } from "devextreme/data";
import CustomStore from "devextreme/data/custom_store";
import DataSource from "devextreme/data/data_source";
import { AppointmentUpdatedEvent } from "devextreme/ui/scheduler";
import { BehaviorSubject } from "rxjs";

import { Pateint } from "../../models/patient/PatientModel";
import { Rdv } from "../../models/rdv/RdvModel";
import { RdvType } from "../../models/rdv/RdvTypeModel";
import { PieceJointe } from "../../models/rdv/Piecejointe";
import { ServiceCmnObject } from "../../services/ServiceCmnObject";
import { ServicesPatient } from "../../services/patient/patient.service";
import { ServicesRdv } from "../../services/rdv/rdv.service";
import notify from "devextreme/ui/notify";
import { VisitDocument } from "../documents/document-info";

const readAsDataUrl = (file: File): Promise<string> => new Promise((resolve, reject) => {
  const reader = new FileReader();
  reader.onload = () => resolve(String(reader.result));
  reader.onerror = () => reject(reader.error);
  reader.readAsDataURL(file);
});

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
  onContextMenuItemClick: any;
  cellContextMenuItems: any[] | undefined;
  appointmentContextMenuItems: any[] | undefined;
  pjListeTemp: VisitDocument[] = [];
  // fires after the visit is saved or finished, so the calendar can redraw it
  @Output() saved = new EventEmitter<void>();
  visitTimeLabel = "";
  birthDateLabel = "";
  exporting = false;

  // a finished visit is a record: shown, exported, but no longer edited
  get finished(): boolean {
    return this.currentRdv?.status === 'TRM';
  }

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

  constructor(
    public readonly PateintService: ServicesPatient,
    public readonly RdvService: ServicesRdv,
    private readonly serviceCmnObject: ServiceCmnObject
  ) {
    this.currentRdv={id:0,patient:{age:"0",firstName:"",lastName:"",birthDate:new Date(Date.now())},startTime:new Date(Date.now())};
    this.pjListeTemp=[];
    this.serviceCmnObject.rdvDetail.subscribe(async rdv=>{
      this.currentRdv=rdv;
      this.pjListeTemp = [];
      this.visitTimeLabel = this.describeTime(rdv);
      this.birthDateLabel = rdv.patient?.birthDate ? new Date(rdv.patient.birthDate).toLocaleDateString('fr-FR') : "";
      if (this.descText)
        this.descText.value = rdv.description ?? "";
      await this.refreshPj();
    })
     
   

  }


  async ngOnInit() {

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

  private describeTime(rdv: Rdv): string {
    if (!rdv.startTime) return "";
    const start = new Date(rdv.startTime);
    const hhmm = (d: Date) => d.toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit' });
    const day = start.toLocaleDateString('fr-FR', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' });
    return rdv.endTime ? `${day}, de ${hhmm(start)} à ${hhmm(new Date(rdv.endTime))}` : `${day}, ${hhmm(start)}`;
  }

  // Downloads the patient's whole file: details, every visit, notes and documents.
  public async exportPdf() {
    const patientId = this.currentRdv.patientId ?? this.currentRdv.patient?.id;
    if (!patientId) return;
    this.exporting = true;
    try {
      const pdf = await this.PateintService.ExportPatientFile(patientId);
      const url = URL.createObjectURL(pdf);
      const link = document.createElement('a');
      link.href = url;
      link.download = `Dossier ${this.currentRdv.patient?.firstName ?? ''} ${this.currentRdv.patient?.lastName ?? ''}.pdf`.replace(/\s+/g, ' ');
      link.click();
      setTimeout(() => URL.revokeObjectURL(url), 1000);
    } catch {
      notify("L'export du dossier a échoué. Réessayez.", 'error', 4000);
    } finally {
      this.exporting = false;
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


 

 

  public getValueDisplay(data: any) {
    return data.firstName + " " + data.lastNime
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
    this.popupVisible = false;


  }

  public async updateRdv(rdv: Rdv) {


    await this.RdvService.UpdateRdv(rdv);


  }

  public async updateDetailRdv() {
    await this.saveVisit(this.currentRdv.status, "Visite enregistrée");
  }

  public async terminerDetailRdv() {
    await this.saveVisit('TRM', "Visite terminée");
  }

  private async saveVisit(status: string | undefined, done: string) {
    const previous = this.currentRdv.status;
    this.currentRdv.description = this.descText.value;
    this.currentRdv.status = status;
    try {
      await this.RdvService.UpdateRdv(this.currentRdv);
      notify(done, "success", 2500);
      this.saved.emit();
    } catch (err: any) {
      this.currentRdv.status = previous;
      notify(err?.error?.message ?? "L'enregistrement de la visite a échoué.", "error", 4000);
    }
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
  try {
    await this.RdvService.DeleteDocument(id);
    notify("Document supprimé", "success", 2000);
  } catch {
    notify("La suppression du document a échoué.", "error", 4000);
  }
  await this.refreshPj();
}

  // Each file is saved as soon as it is picked, then the gallery reloads from the server.
  public async filesUploaded(e: any)
  {
    const files: File[] = e.value ?? [];
    if (!files.length || !this.currentRdv.id) return;

    let added = 0;
    for (const file of files) {
      try {
        const file64 = await readAsDataUrl(file);
        await this.RdvService.AddDocument({ visitId: this.currentRdv.id, fileName: encodeURI(file.name), file64 });
        added++;
      } catch {
        notify(`« ${file.name} » n'a pas pu être enregistré.`, "error", 4000);
      }
    }
    if (added)
      notify(added > 1 ? `${added} documents ajoutés` : "Document ajouté", "success", 2500);

    e.component.reset(); // clears the picker; the change event it fires has no files and returns above
    await this.refreshPj();
  }

}

