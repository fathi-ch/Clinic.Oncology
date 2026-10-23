// The clinic's details, shown in the header, footer and contact page.
// The PDF letterhead (BackEnd Clinic.Core/Services/ClinicLetterhead.cs) holds the same values; change both together.
export const CLINIC = {
  practice: "Cabinet d'oncologie médicale",
  doctor: 'Dr Amrane L.',
  specialty: 'Oncologue médicale',
  doctorArabic: 'الدكتورة عمران ل.',
  specialtyArabic: 'أخصائية طب الأورام والسرطان',
  address: 'Cité Chikh Larbi Tebessi, lot 130 n°1, Tébessa',
  phone: '0 790 104 204',
  phoneHref: 'tel:+213790104204', // Algerian mobile in international format
  hours: 'Consultations de 08:00 à 17:00',
  mapHref: 'https://www.google.com/maps/search/?api=1&query=' +
    encodeURIComponent('Cité Chikh Larbi Tebessi, Tébessa, Algérie'),
} as const;
