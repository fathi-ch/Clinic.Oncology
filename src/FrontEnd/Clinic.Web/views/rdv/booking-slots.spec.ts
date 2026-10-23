import { Rdv } from '../../models/rdv/RdvModel';
import { BookingSlots } from './booking-slots';

describe('BookingSlots', () => {
  const day = new Date(2026, 9, 29);
  const at = (h: number, m = 0) => new Date(2026, 9, 29, h, m);
  const visit = (id: number, start: Date, end: Date): Rdv => ({ id, startTime: start, endTime: end });
  const texts = (o: { text: string }[]) => o.map(x => x.text);

  it('offers only free starts and keeps the wanted start and end when they are free', async () => {
    const slots = new BookingSlots(async () => [visit(1, at(9, 30), at(10))]);
    await slots.load(day, at(9), at(9, 30));
    expect(texts(slots.startOptions)).not.toContain('09:30');
    expect(texts(slots.startOptions)).not.toContain('09:45');
    expect(slots.startValue).toBe(at(9).getTime());
    expect(texts(slots.endOptions)).toEqual(['09:15', '09:30']);
    expect(slots.endValue).toBe(at(9, 30).getTime());
  });

  it('moves a wanted start that is booked to the next free slot', async () => {
    const slots = new BookingSlots(async () => [visit(1, at(9, 30), at(10))]);
    await slots.load(day, at(9, 30), null);
    expect(slots.startValue).toBe(at(10).getTime());
    expect(slots.endValue).toBe(at(10, 15).getTime());
  });

  it('ignores the visit being edited so its own slot stays available', async () => {
    const slots = new BookingSlots(async () => [visit(7, at(9, 30), at(10))]);
    await slots.load(day, at(9, 30), at(10), 7);
    expect(slots.startValue).toBe(at(9, 30).getTime());
    expect(slots.endValue).toBe(at(10).getTime());
  });

  it('a start picked by the user moves the end to one slot later', async () => {
    const slots = new BookingSlots(async () => []);
    await slots.load(day, at(9), at(10));
    slots.onStartChanged({ value: at(11).getTime(), event: {} });
    expect(slots.endValue).toBe(at(11, 15).getTime());
  });

  it('clears everything without a day', async () => {
    const slots = new BookingSlots(async () => []);
    await slots.load(null, null, null);
    expect(slots.startOptions).toEqual([]);
    expect(slots.startValue).toBeNull();
    expect(slots.endValue).toBeNull();
  });
});
