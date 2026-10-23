import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { custom } from 'devextreme/ui/dialog';
import { addedAt, isPdf, mimeOf, toBlob, VisitDocument } from './document-info';

interface Slide {
  doc: VisitDocument;
  src: string;
  pdf: boolean;
  label: string;
  added: string;
  context: string;
}

// Carousel for a visit's photos and PDFs: main view, thumbnail strip and a full-screen viewer.
@Component({
  selector: 'app-document-gallery',
  templateUrl: './document-gallery.component.html',
  styleUrls: ['./document-gallery.component.scss'],
})
export class DocumentGalleryComponent implements OnChanges {
  @Input() documents: VisitDocument[] = [];
  @Input() readonly = false;
  // extra line under each document, keyed by visit id (the patient page shows which visit it belongs to)
  @Input() visitLabels: Record<number, string> = {};
  @Output() remove = new EventEmitter<number>();

  slides: Slide[] = [];
  index = 0;
  viewerVisible = false;
  // the viewer carousel is created once its popup has finished opening, so it measures its real width
  viewerReady = false;

  ngOnChanges(changes: SimpleChanges): void {
    const previous: VisitDocument[] = changes['documents']?.previousValue ?? [];
    // Built once per change: data URLs of photos are large, so templates must not rebuild them.
    this.slides = (this.documents ?? []).map(doc => this.toSlide(doc));

    const sameVisit = previous.length > 0 && previous[0]?.id === this.documents?.[0]?.id;
    if (!sameVisit) this.index = 0;                                                   // another visit: start at the first
    else if (this.slides.length > previous.length) this.index = this.slides.length - 1; // just uploaded: show it
    else this.index = Math.min(this.index, Math.max(0, this.slides.length - 1));      // deleted: stay in range
  }

  get current(): Slide | undefined {
    return this.slides[this.index];
  }

  openPdf(slide: Slide): void {
    window.open(URL.createObjectURL(toBlob(slide.doc)), '_blank');
  }

  async confirmRemove(slide: Slide): Promise<void> {
    const ok = await custom({
      title: 'Supprimer le document',
      messageHtml: 'Supprimer ce document ? Cette action est définitive.',
      buttons: [
        { text: 'Annuler', onClick: () => false },
        { text: 'Supprimer', type: 'danger', onClick: () => true },
      ],
    }).show();
    if (ok) this.remove.emit(slide.doc.id);
  }

  private toSlide(doc: VisitDocument): Slide {
    const pdf = isPdf(doc.documentType);
    const date = addedAt(doc.name);
    return {
      doc,
      pdf,
      src: pdf ? '' : `data:${mimeOf(doc.documentType)};base64,${doc.patientDocumentsbase64}`,
      label: pdf ? 'Document PDF' : 'Photo',
      context: this.visitLabels[doc.visitId] ?? '',
      added: date
        ? `Ajouté le ${date.toLocaleDateString('fr-FR')} à ${date.toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit' })}`
        : '',
    };
  }
}
