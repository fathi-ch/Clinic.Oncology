// GET /statistics/income. Amounts in DA; income counts finished visits billed "Payant" or "Promotion".
export interface IncomeTotals {
  paid: number;
  promo: number;
  income: number;
  planned: number;
  finishedVisits: number;
  freeVisits: number;
}

export interface IncomeBucket extends IncomeTotals {
  key: string; // "2026-10-24", "2026-10" or "2026"
}

export interface PatientIncome extends IncomeTotals {
  patientId: number;
  name: string;
}

export interface IncomeReport {
  from: string;
  to: string;
  period: IncomeTotals;
  today: number;
  thisMonth: number;
  thisYear: number;
  byDay: IncomeBucket[];
  byMonth: IncomeBucket[];
  byYear: IncomeBucket[];
  byPatient: PatientIncome[];
}
