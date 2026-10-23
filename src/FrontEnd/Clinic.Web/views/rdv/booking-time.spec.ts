import { availableEnds, availableStarts, endAfterStart, withTimeOf } from './booking-time';

describe('booking-time', () => {
  const day = new Date(2026, 9, 27);
  const at = (h: number, m = 0) => new Date(2026, 9, 27, h, m);
  const hhmm = (dates: Date[]) => dates.map(d => d.toTimeString().slice(0, 5));
  const booked = [
    { start: at(9), end: at(9, 30) },
    { start: at(10), end: at(10, 15) },
    { start: at(11, 10), end: at(11, 20) },
  ];

  it('withTimeOf puts the time on the booking day', () => {
    expect(withTimeOf(day, new Date(1900, 0, 1, 8, 30))).toEqual(at(8, 30));
  });

  it('endAfterStart adds one slot', () => {
    expect(endAfterStart(at(16, 45))).toEqual(at(17));
  });

  it('availableStarts lists 08:00 to 16:45 on a free day', () => {
    const starts = hhmm(availableStarts(day, []));
    expect(starts.length).toBe(36);
    expect(starts[0]).toBe('08:00');
    expect(starts[starts.length - 1]).toBe('16:45');
  });

  it('availableStarts skips slots that overlap a booking', () => {
    const starts = hhmm(availableStarts(day, booked));
    expect(starts).not.toContain('09:00');
    expect(starts).not.toContain('09:15');
    expect(starts).not.toContain('10:00');
    expect(starts).not.toContain('11:00');
    expect(starts).not.toContain('11:15');
    expect(starts).toContain('08:45');
    expect(starts).toContain('09:30');
    expect(starts).toContain('10:15');
    expect(starts).toContain('11:30');
  });

  it('availableEnds stops at the next booking', () => {
    expect(hhmm(availableEnds(at(8, 30), booked))).toEqual(['08:45', '09:00']);
    expect(hhmm(availableEnds(at(9, 30), booked))).toEqual(['09:45', '10:00']);
    expect(hhmm(availableEnds(at(10, 45), booked))).toEqual(['11:00']);
  });

  it('availableEnds stops at 17:00 when nothing is booked after the start', () => {
    expect(hhmm(availableEnds(at(16, 30), booked))).toEqual(['16:45', '17:00']);
  });
});
