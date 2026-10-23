import { addedAt, isPdf, mimeOf } from './document-info';

describe('document-info', () => {
  it('reads the upload time (UTC) from the stored file name', () => {
    expect(addedAt('5_20261023_211854_63928394334679.png')).toEqual(new Date(Date.UTC(2026, 9, 23, 21, 18, 54)));
  });

  it('returns null for a name that does not follow the storage pattern', () => {
    expect(addedAt('scan.png')).toBeNull();
  });

  it('maps file types to MIME types, case-insensitively', () => {
    expect(mimeOf('.png')).toBe('image/png');
    expect(mimeOf('.JPG')).toBe('image/jpeg');
    expect(mimeOf('.jpeg')).toBe('image/jpeg');
    expect(mimeOf('.pdf')).toBe('application/pdf');
  });

  it('recognises PDFs', () => {
    expect(isPdf('.pdf')).toBeTrue();
    expect(isPdf('.PDF')).toBeTrue();
    expect(isPdf('.png')).toBeFalse();
  });
});
