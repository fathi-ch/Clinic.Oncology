import { Rdv } from '../../models/rdv/RdvModel';
import { availableEnds, availableStarts, Interval, withTimeOf } from './booking-time';

export interface TimeOption {
  value: number;
  text: string;
}

const toTimeOption = (d: Date): TimeOption => ({
  value: d.getTime(),
  text: d.toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit' }),
});

// State behind a booking form's "de" / "à" lists: only free slots of the chosen day are offered.
// Shared by the calendar's booking popup and the new-patient popup.
export class BookingSlots {
  startOptions: TimeOption[] = [];
  endOptions: TimeOption[] = [];
  startValue: number | null = null;
  endValue: number | null = null;
  private booked: Interval[] = [];
  private request = 0;

  constructor(private readonly visitsOn: (day: Date) => Promise<Rdv[] | undefined>) {}

  // Loads the day's bookings, keeping the wanted times when they are free.
  // `ignoreVisitId` is the visit being edited, whose own slot must stay available.
  async load(day: Date | null, start: Date | null, end: Date | null, ignoreVisitId?: number): Promise<void> {
    const request = ++this.request;
    this.booked = [];
    this.startOptions = [];
    this.selectStart(null);
    if (!day) return;

    const visits = (await this.visitsOn(day)) ?? [];
    if (request !== this.request) return; // another day was picked meanwhile

    this.booked = visits
      .filter(v => v.id !== ignoreVisitId && v.startTime && v.endTime)
      .map(v => ({ start: new Date(v.startTime!), end: new Date(v.endTime!) }));
    this.startOptions = availableStarts(day, this.booked).map(toTimeOption);
    const wanted = start ? withTimeOf(day, start).getTime() : null;
    const first = this.startOptions.find(o => wanted === null || o.value >= wanted);
    this.selectStart(first?.value ?? null, end ? withTimeOf(day, end).getTime() : null);
  }

  // Bound to the "de" list. Values set by code (no event) are already handled by load().
  onStartChanged(e: { value?: number | null; event?: unknown }): void {
    if (e.event) this.selectStart(e.value ?? null);
  }

  // Current start and end as dates, or null while either is missing.
  chosen(): { start: Date; end: Date } | null {
    return this.startValue === null || this.endValue === null
      ? null
      : { start: new Date(this.startValue), end: new Date(this.endValue) };
  }

  private selectStart(start: number | null, wantedEnd: number | null = null): void {
    this.startValue = start;
    this.endOptions = start === null ? [] : availableEnds(new Date(start), this.booked).map(toTimeOption);
    this.endValue = this.endOptions.find(o => o.value === wantedEnd)?.value ?? this.endOptions[0]?.value ?? null;
  }
}
