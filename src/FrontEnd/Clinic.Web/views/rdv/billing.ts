// How a visit is billed, chosen at booking. The API stores FREE visits at 0 DA.
export type BillingType = 'PAID' | 'PROMO' | 'FREE';

export const BILLING_OPTIONS: { value: BillingType; text: string }[] = [
  { value: 'PAID', text: 'Payant' },
  { value: 'PROMO', text: 'Promotion' },
  { value: 'FREE', text: 'Gratuit' },
];

export const billingLabel = (type?: string): string =>
  BILLING_OPTIONS.find(o => o.value === type)?.text ?? 'Payant';
