// Booking slots: 15 minutes, 08:00 to 17:00, matching the scheduler's startDayHour/endDayHour.
export const SLOT_MINUTES = 15;
const DAY_START_HOUR = 8;
const DAY_END_HOUR = 17;

export interface Interval {
  start: Date;
  end: Date;
}

const atTime = (day: Date, hours: number, minutes = 0): Date => {
  const d = new Date(day);
  d.setHours(hours, minutes, 0, 0);
  return d;
};

const overlaps = (start: Date, end: Date, booked: Interval[]): boolean =>
  booked.some(b => start < b.end && end > b.start);

export function withTimeOf(day: Date, time: Date): Date {
  return atTime(day, time.getHours(), time.getMinutes());
}

export function endAfterStart(start: Date): Date {
  return new Date(start.getTime() + SLOT_MINUTES * 60_000);
}

// Start slots on `day` whose first 15 minutes are free.
export function availableStarts(day: Date, booked: Interval[]): Date[] {
  const starts: Date[] = [];
  const close = atTime(day, DAY_END_HOUR);
  for (let s = atTime(day, DAY_START_HOUR); s < close; s = endAfterStart(s)) {
    if (!overlaps(s, endAfterStart(s), booked)) starts.push(s);
  }
  return starts;
}

// End slots after `start`, up to the next booking or closing time, whichever comes first.
export function availableEnds(start: Date, booked: Interval[]): Date[] {
  const nextBooking = booked
    .map(b => b.start)
    .filter(s => s > start)
    .reduce((min, s) => (s < min ? s : min), atTime(start, DAY_END_HOUR));
  const ends: Date[] = [];
  for (let e = endAfterStart(start); e <= nextBooking; e = endAfterStart(e)) ends.push(e);
  return ends;
}
