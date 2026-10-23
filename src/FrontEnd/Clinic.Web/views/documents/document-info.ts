// A visit document as returned by GET /visits/{id}/documents.
export interface VisitDocument {
  id: number;
  visitId: number;
  name: string;          // stored name: {visitId}_{yyyyMMdd}_{HHmmss}_{ms}{ext}, time in UTC
  documentType: string;  // file extension, e.g. ".png"
  patientDocumentsbase64: string;
}

const MIME: Record<string, string> = {
  '.png': 'image/png',
  '.jpg': 'image/jpeg',
  '.jpeg': 'image/jpeg',
  '.pdf': 'application/pdf',
};

export const mimeOf = (type: string): string => MIME[type.toLowerCase()] ?? 'application/octet-stream';

export const isPdf = (type: string): boolean => type.toLowerCase() === '.pdf';

// The server keeps no upload date column; the stored name carries it.
export function addedAt(name: string): Date | null {
  const m = /^\d+_(\d{4})(\d{2})(\d{2})_(\d{2})(\d{2})(\d{2})_/.exec(name);
  return m ? new Date(Date.UTC(+m[1], +m[2] - 1, +m[3], +m[4], +m[5], +m[6])) : null;
}

export function toBlob(doc: VisitDocument): Blob {
  const bytes = Uint8Array.from(atob(doc.patientDocumentsbase64), c => c.charCodeAt(0));
  return new Blob([bytes], { type: mimeOf(doc.documentType) });
}
