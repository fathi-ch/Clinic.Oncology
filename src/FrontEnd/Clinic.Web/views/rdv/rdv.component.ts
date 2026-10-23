import { Component, EventEmitter, NgModule, OnInit, Output, ViewChild } from "@angular/core";
import { BrowserModule } from "@angular/platform-browser";
import { Router } from "@angular/router";
import { DxBulletModule, DxDataGridComponent, DxDataGridModule, DxDateBoxComponent, DxSchedulerComponent, DxTemplateModule, DxTextBoxComponent, DxContextMenuComponent, DxAutocompleteComponent, DxNumberBoxComponent } from "devextreme-angular";
import { DxSchedulerTypes } from "devextreme-angular/ui/scheduler";
import { LoadOptions } from "devextreme/data";
import CustomStore from "devextreme/data/custom_store";
import DataSource from "devextreme/data/data_source";
import { AppointmentUpdatedEvent } from "devextreme/ui/scheduler";
import { BehaviorSubject } from "rxjs";

import { Pateint } from "../../models/patient/PatientModel";
import { Rdv } from "../../models/rdv/RdvModel";
import { RdvType } from "../../models/rdv/RdvTypeModel";
import { ServiceCmnObject } from "../../services/ServiceCmnObject";
import { ServicesPatient } from "../../services/patient/patient.service";
import { ServicesRdv } from "../../services/rdv/rdv.service";
import notify from "devextreme/ui/notify";
import { BookingSlots } from "./booking-slots";
import { BILLING_OPTIONS, BillingType } from "./billing";

@Component({
  selector: 'app-rdv',
  templateUrl: './rdv.component.html',
  styleUrls: ['./rdv.component.scss']

})


export class RdvComponent implements OnInit {

  @ViewChild("dateNais", { static: false }) dateNais!: DxDateBoxComponent;
  @ViewChild("dateRdv", { static: false }) dateRdv!: DxDateBoxComponent;
  @ViewChild("scheduler_ges", { static: false }) scheduler_ges!: DxSchedulerComponent;
  @ViewChild('contextMenu', { static: false }) contextMenu!: DxContextMenuComponent;
  @ViewChild('patientRdv', { static: false }) patientRdv!: DxAutocompleteComponent;
  @ViewChild('prixRdv', { static: false }) prixRdv!: DxNumberBoxComponent;
  



  // fires after each calendar load, so the home page cards can follow new bookings
  @Output() calendarLoaded = new EventEmitter<void>();

  // 08:00-17:00 in 30-minute rows of 36px (see rdv.component.scss), plus toolbar, day headers and the
  // work space padding: the whole working day fits without scrolling inside the calendar.
  readonly calendarHeight = 18 * 36 + 166;

  toAdd: boolean = true;
  rdvData: DataSource;
  onContextMenuItemClick: any;
  cellContextMenuItems: any[] | undefined;
  appointmentContextMenuItems: any[] | undefined;

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
  readonly billingOptions = BILLING_OPTIONS;
  billingType: BillingType = 'PAID';
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
  // the booking form's "de" / "à" lists, built from the selected day's free slots
  slots = new BookingSlots(day => this.RdvService.GetRdvByDate(day, day));
  dateClear = new Date(2015, 11, 1, 6);

  timeMin: Date = new Date(0, 0, 0, 7, 0, 0);
  timeMax: Date = new Date(0, 0, 0, 22, 0, 0);
  currentRdv = new Rdv();

  public currentDate: Date = new Date();

  searchText: string = "";

  newRdvSelectedPatien: Pateint = new Pateint();


  // data grid data source 
  LstPatients: Pateint[] = [];
  popupRdvDetVisible=false;

  constructor(
    public readonly PateintService: ServicesPatient,
    public readonly RdvService: ServicesRdv,
    private readonly serviceCmnObject: ServiceCmnObject,
    private readonly router: Router
  ) {
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

          that.calendarLoaded.emit();
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

  }

  public checkRdvStatu(status:string)
  {
    return status=='PLA';
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
    this.slots.load(null, null, null);
    this.popupVisible = true;
    this.patientRdv.value = "";
    this.newRdvSelectedPatien = new Pateint();
    this.prixRdv.value=0;
    this.billingType = 'PAID';
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
      this.slots.load(null, null, null);
      this.prixRdv.value=0;
    }


  }

  public async newRdv() {
    const chosen = this.slots.chosen();
    if (!chosen) {
      notify("Choisissez l'heure de début et de fin.", 'warning', 3000);
      return;
    }
    const startTime = chosen.start;
    const endTime = chosen.end;
    let _patien = this.newRdvSelectedPatien;
    let _selectedType = this.selectedType;

    let _rdv = new Rdv();
    _rdv.patientId = this.newRdvSelectedPatien.id;
    _rdv.startTime = startTime;
    _rdv.endTime = endTime;
    _rdv.price = this.billingType === 'FREE' ? 0 : this.rdvPrix;
    _rdv.billingType = this.billingType;
    _rdv.status = "PLA";
    _rdv.visitType = this.selectedType;
    _rdv.description = "";
    _rdv.weight=0;
    _rdv.height=0;
    _rdv.visitDateTitel="";
    if (!(await this.saved(this.RdvService.NewRdv(_rdv)))) return;
    this.rdvData.reload();
    this.popupVisible = false;


  }

  get isFinished(): boolean {
    return this.currentRdv?.status === 'TRM';
  }

  public getToRdv()
  {
    this.serviceCmnObject.rdvDetail.next(this.currentRdv);
    this.popupVisible = false; // the visit details replace the booking form
    this.popupRdvDetVisible=true;
   // this.router.navigate(['/rdvDetails']);
  }
  public async updateRdvForm() {
    let _rdv = this.currentRdv;
    const chosen = this.slots.chosen();
    if (!chosen) {
      notify("Choisissez l'heure de début et de fin.", 'warning', 3000);
      return;
    }
    const startTime = chosen.start;
    const endTime = chosen.end;
    let _patien = this.newRdvSelectedPatien;
    let _selectedType = this.selectedType;


    _rdv.patientId = this.newRdvSelectedPatien.id;
    _rdv.startTime = startTime;
    _rdv.endTime = endTime;
    _rdv.price = this.billingType === 'FREE' ? 0 : this.rdvPrix;
    _rdv.billingType = this.billingType;
    _rdv.weight=this.currentRdv.weight;
    _rdv.height=this.currentRdv.height;
    // rescheduling keeps what the visit already holds: its status and its consultation notes
    _rdv.status = _rdv.status ?? "PLA";
    _rdv.visitType = this.selectedType;
    _rdv.visitDateTitel="";
    if (!(await this.saved(this.RdvService.UpdateRdv(_rdv)))) return;
    this.rdvData.reload();
    this.popupVisible = false;


  }

  public async updateRdv(rdv: Rdv) {


    await this.saved(this.RdvService.UpdateRdv(rdv));
    this.rdvData.reload();


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
    e.cancel = true;
    const rdv = e.appointmentData as Rdv;
    this.currentRdv = rdv;
    this.toAdd = !rdv.id;
    this.dateRdv.instance.reset();
    this.patientRdv.value = "";
    this.newRdvSelectedPatien = new Pateint();
    this.prixRdv.value = 0;

    if (rdv.id) {
      if (rdv.patient) {
        this.newRdvSelectedPatien = rdv.patient;
        this.patientRdv.value = rdv.patient.autocpmliteValue ? rdv.patient.autocpmliteValue : "";
      }
      if (rdv.price)
        this.prixRdv.value = rdv.price;
    }
    this.selectedType = rdv.visitType ?? "";
    this.billingType = rdv.billingType ?? 'PAID';

    const start = rdv.startTime ? new Date(rdv.startTime) : null;
    if (start)
      this.dateRdv.value = start;
    this.slots.load(start, start, rdv.endTime ? new Date(rdv.endTime) : null, rdv.id);
    this.popupVisible = true;
  }

  // "Nouveau rdv" from the home page: today, first free slot from now on
  public openNewBooking() {
    const rdv = new Rdv();
    rdv.startTime = new Date();
    this.onAppointmentFormOpening({ appointmentData: rdv });
  }

  // a free visit is booked at 0 DA; the price box is locked while "Gratuit" is chosen
  public onBillingChanged(type: BillingType) {
    if (type === 'FREE') this.prixRdv.value = 0;
  }

  public onDateChanged(e: any) {
    // dates set by code (opening a visit or a cell) load their slots themselves
    if (!e.event) return;
    const keep = (v: number | null) => (v === null ? null : new Date(v));
    this.slots.load(e.value ? new Date(e.value) : null, keep(this.slots.startValue), keep(this.slots.endValue), this.currentRdv?.id);
  }

  // Waits for a save and reports a refusal from the API instead of failing silently.
  private async saved(request: Promise<unknown>): Promise<boolean> {
    try {
      await request;
      return true;
    } catch (err: any) {
      const message = err?.status === 409
        ? "Ce créneau vient d'être réservé. Choisissez un autre horaire."
        : err?.error?.message ?? "L'enregistrement du rdv a échoué.";
      notify(message, 'error', 4000);
      return false;
    }
  }

  public handlePropertyChange(e: any) {
    e.cancel = true;

  }

}

