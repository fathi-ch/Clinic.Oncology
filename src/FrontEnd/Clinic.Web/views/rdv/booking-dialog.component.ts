import { Component, EventEmitter, Output } from '@angular/core';
import notify from 'devextreme/ui/notify';
import { Pateint } from '../../models/patient/PatientModel';
import { Rdv } from '../../models/rdv/RdvModel';
import { ServicesRdv } from '../../services/rdv/rdv.service';
import { BILLING_OPTIONS, BillingType } from './billing';
import { BookingSlots } from './booking-slots';

// Booking for a known patient without going through the calendar (patient page, patients grid).
// Same free-slot lists and billing choice as the calendar's booking form.
@Component({
  selector: 'app-booking-dialog',
  templateUrl: './booking-dialog.component.html',
  styleUrls: ['./booking-dialog.component.scss'],
})
export class BookingDialogComponent {
  @Output() booked = new EventEmitter<void>();

  visible = false;
  patient: Pateint | null = null;
  day = new Date();
  billing: BillingType = 'PAID';
  price = 0;
  visitType = 'RDV';
  saving = false;
  readonly today = new Date(new Date().setHours(0, 0, 0, 0));
  readonly billingOptions = BILLING_OPTIONS;
  readonly visitTypes = [{ code: 'RDV', type: 'Rdv médical' }, { code: 'ABS', type: 'Absent' }];
  readonly slots: BookingSlots;

  constructor(private readonly visits: ServicesRdv) {
    this.slots = new BookingSlots(day => this.visits.GetRdvByDate(day, day));
  }

  get patientName(): string {
    return `${this.patient?.firstName ?? ''} ${this.patient?.lastName ?? ''}`.trim();
  }

  open(patient: Pateint | null): void {
    if (!patient?.id) return;
    this.patient = patient;
    this.day = new Date();
    this.billing = 'PAID';
    this.price = 0;
    this.visitType = 'RDV';
    this.slots.load(this.day, new Date(), null); // first free slot from now on
    this.visible = true;
  }

  onDayChanged(e: { value?: Date | string | number | null; event?: unknown }): void {
    if (!e.event) return;
    if (e.value) this.day = new Date(e.value);
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
      price: this.billing === 'FREE' ? 0 : this.price ?? 0,
      billingType: this.billing,
      status: 'PLA',
      visitType: this.visitType,
      description: '',
      weight: 0,
      height: 0,
      visitDateTitel: '',
    };
    this.saving = true;
    try {
      await this.visits.NewRdv(visit);
      notify('Rendez-vous ajouté', 'success', 2500);
      this.visible = false;
      this.booked.emit();
    } catch (err: any) {
      notify(err?.status === 409 ? "Ce créneau vient d'être réservé. Choisissez un autre horaire." : err?.error?.message ?? 'La réservation a échoué.', 'error', 4000);
    } finally {
      this.saving = false;
    }
  }
}
