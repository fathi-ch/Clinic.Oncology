import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import notify from 'devextreme/ui/notify';
import { firstValueFrom, Subscription } from 'rxjs';
import { Pateint } from '../../models/patient/PatientModel';
import { Rdv } from '../../models/rdv/RdvModel';
import { ServiceCmnObject } from '../../services/ServiceCmnObject';
import { ServicesPatient } from '../../services/patient/patient.service';
import { ServicesRdv } from '../../services/rdv/rdv.service';
import { BILLING_OPTIONS, BillingType } from '../rdv/billing';
import { BookingSlots } from '../rdv/booking-slots';
import { isPdf, mimeOf, VisitDocument } from '../documents/document-info';

interface VisitRow {
  visit: Rdv;
  when: string;
  done: boolean;
  documents: number;
}

const day = (d: Date) => d.toLocaleDateString('fr-FR', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' });
const time = (d: Date) => d.toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit' });

// Everything about one patient on one page: details, all documents, every visit.
@Component({
  selector: 'app-patient-file',
  templateUrl: './patient-file.component.html',
  styleUrls: ['./patient-file.component.scss'],
})
export class PatientFileComponent implements OnInit, OnDestroy {
  patient: Pateint | null = null;
  notFound = false;
  rows: VisitRow[] = [];
  documents: VisitDocument[] = [];
  // the patient's photos (not PDFs) for the carousel at the top of the page
  photos: { src: string; label: string }[] = [];
  visitLabels: Record<number, string> = {};
  doneCount = 0;
  nextVisitDay = '';
  nextVisitTime = '';
  exporting = false;
  editVisible = false;

  // booking from this page, without going through the calendar
  bookingVisible = false;
  bookingDay: Date = new Date();
  bookingBilling: BillingType = 'PAID';
  bookingPrice = 0;
  bookingType = 'RDV';
  booking = false;
  readonly today = new Date(new Date().setHours(0, 0, 0, 0));
  readonly billingOptions = BILLING_OPTIONS;
  readonly visitTypes = [{ code: 'RDV', type: 'Rdv médical' }, { code: 'ABS', type: 'Absent' }];
  readonly slots: BookingSlots;
  visitVisible = false;

  private id = 0;
  private routeSub?: Subscription;

  constructor(
    private readonly route: ActivatedRoute,
    public readonly router: Router,
    private readonly patients: ServicesPatient,
    private readonly shared: ServiceCmnObject,
    private readonly visits: ServicesRdv,
  ) {
    this.slots = new BookingSlots(day => this.visits.GetRdvByDate(day, day));
  }

  ngOnInit(): void {
    this.routeSub = this.route.paramMap.subscribe(params => {
      this.id = Number(params.get('id'));
      this.load();
    });
  }

  ngOnDestroy(): void {
    this.routeSub?.unsubscribe();
  }

  get fullName(): string {
    return `${this.patient?.firstName ?? ''} ${this.patient?.lastName ?? ''}`.trim();
  }

  get birthLabel(): string {
    return this.patient?.birthDate ? new Date(this.patient.birthDate).toLocaleDateString('fr-FR') : '';
  }

  async load(): Promise<void> {
    try {
      const [patient, visits, documents] = await Promise.all([
        this.patients.GetPatient(this.id),
        firstValueFrom(this.patients.GetAllVisitesByPateint(this.id)),
        this.patients.GetPatientDocuments(this.id),
      ]);
      this.patient = patient;
      this.notFound = false;
      this.documents = documents;
      this.rows = visits.map(v => this.toRow(v, documents));
      this.doneCount = this.rows.filter(r => r.done).length;
      this.visitLabels = Object.fromEntries(this.rows.map(r => [r.visit.id, `Visite du ${r.when}`]));
      this.photos = documents
        .filter(d => !isPdf(d.documentType))
        .map(d => ({ src: `data:${mimeOf(d.documentType)};base64,${d.patientDocumentsbase64}`, label: this.visitLabels[d.visitId] ?? '' }));
      const now = Date.now();
      // the next planned visit; a visit already marked finished is not upcoming
      const next = [...visits]
        .filter(v => v.status !== 'TRM' && v.startTime && new Date(v.startTime).getTime() >= now)
        .sort((a, b) => new Date(a.startTime!).getTime() - new Date(b.startTime!).getTime())[0];
      const nextStart = next ? new Date(next.startTime!) : null;
      this.nextVisitDay = nextStart ? nextStart.toLocaleDateString('fr-FR', { weekday: 'long', day: 'numeric', month: 'long' }).replace(/^./, c => c.toUpperCase()) : '';
      this.nextVisitTime = nextStart ? `à ${time(nextStart)}` : '';
    } catch (err: any) {
      this.patient = null;
      this.notFound = err?.status === 404;
      if (!this.notFound) notify("Le dossier du patient n'a pas pu être chargé.", 'error', 4000);
    }
  }

  editPatient(): void {
    if (!this.patient) return;
    this.shared.patientDetail.next(this.patient);
    this.editVisible = true;
  }

  onPatientDeleted(): void {
    this.editVisible = false;
    this.router.navigate(['/patients']);
  }

  openBooking(): void {
    this.bookingDay = new Date();
    this.bookingBilling = 'PAID';
    this.bookingPrice = 0;
    this.bookingType = 'RDV';
    this.slots.load(this.bookingDay, new Date(), null); // first free slot from now on
    this.bookingVisible = true;
  }

  onBookingDayChanged(e: { value?: Date | string | number | null; event?: unknown }): void {
    if (!e.event) return;
    if (e.value) this.bookingDay = new Date(e.value);
    const keep = (v: number | null) => (v === null ? null : new Date(v));
    this.slots.load(e.value ? new Date(e.value) : null, keep(this.slots.startValue), keep(this.slots.endValue));
  }

  async book(): Promise<void> {
    const chosen = this.slots.chosen();
    if (!this.patient?.id || !chosen) {
      notify("Choisissez la date, l'heure de début et de fin.", 'warning', 3000);
      return;
    }
    const visit: Rdv = {
      patientId: this.patient.id,
      startTime: chosen.start,
      endTime: chosen.end,
      price: this.bookingBilling === 'FREE' ? 0 : this.bookingPrice ?? 0,
      billingType: this.bookingBilling,
      status: 'PLA',
      visitType: this.bookingType,
      description: '',
      weight: 0,
      height: 0,
      visitDateTitel: '',
    };
    this.booking = true;
    try {
      await this.visits.NewRdv(visit);
      notify('Rendez-vous ajouté', 'success', 2500);
      this.bookingVisible = false;
      await this.load();
    } catch (err: any) {
      notify(err?.status === 409 ? "Ce créneau vient d'être réservé. Choisissez un autre horaire." : err?.error?.message ?? "La réservation a échoué.", 'error', 4000);
    } finally {
      this.booking = false;
    }
  }

  openVisit(row: VisitRow): void {
    this.shared.rdvDetail.next({ ...row.visit, patient: this.patient ?? undefined });
    this.visitVisible = true;
  }

  async exportPdf(): Promise<void> {
    this.exporting = true;
    try {
      const pdf = await this.patients.ExportPatientFile(this.id);
      const url = URL.createObjectURL(pdf);
      const link = document.createElement('a');
      link.href = url;
      link.download = `Dossier ${this.fullName}.pdf`;
      link.click();
      setTimeout(() => URL.revokeObjectURL(url), 1000);
    } catch {
      notify("L'export du dossier a échoué. Réessayez.", 'error', 4000);
    } finally {
      this.exporting = false;
    }
  }

  private toRow(visit: Rdv, documents: VisitDocument[]): VisitRow {
    const start = visit.startTime ? new Date(visit.startTime) : null;
    const end = visit.endTime ? new Date(visit.endTime) : null;
    return {
      visit,
      when: start ? `${day(start)}, ${time(start)}${end ? ' à ' + time(end) : ''}` : '',
      done: visit.status === 'TRM',
      documents: documents.filter(d => d.visitId === visit.id).length,
    };
  }
}
