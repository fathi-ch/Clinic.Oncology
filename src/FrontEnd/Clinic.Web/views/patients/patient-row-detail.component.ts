import { Component, Input, OnInit } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { Rdv } from '../../models/rdv/RdvModel';
import { ServicesPatient } from '../../services/patient/patient.service';
import { isPdf, mimeOf } from '../documents/document-info';
import { billingLabel } from '../rdv/billing';

interface VisitLine {
  when: string;
  done: boolean;
  billing: string;
  price: string;
}

// What a patient row shows when expanded in the patients grid: visits and documents, loaded on demand.
@Component({
  selector: 'app-patient-row-detail',
  templateUrl: './patient-row-detail.component.html',
  styleUrls: ['./patient-row-detail.component.scss'],
})
export class PatientRowDetailComponent implements OnInit {
  @Input() patientId!: number;

  loading = true;
  failed = false;
  visits: VisitLine[] = [];
  thumbs: { src: string; pdf: boolean }[] = [];
  documentCount = 0;

  constructor(private readonly patients: ServicesPatient) {}

  async ngOnInit(): Promise<void> {
    try {
      const [visits, documents] = await Promise.all([
        firstValueFrom(this.patients.GetAllVisitesByPateint(this.patientId)),
        this.patients.GetPatientDocuments(this.patientId),
      ]);
      this.visits = visits.map(v => this.toLine(v));
      this.documentCount = documents.length;
      this.thumbs = documents.slice(0, 8).map(d => ({
        pdf: isPdf(d.documentType),
        src: isPdf(d.documentType) ? '' : `data:${mimeOf(d.documentType)};base64,${d.patientDocumentsbase64}`,
      }));
    } catch {
      this.failed = true;
    } finally {
      this.loading = false;
    }
  }

  private toLine(v: Rdv): VisitLine {
    const start = v.startTime ? new Date(v.startTime) : null;
    return {
      when: start
        ? `${start.toLocaleDateString('fr-FR')} à ${start.toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit' })}`
        : '-',
      done: v.status === 'TRM',
      billing: billingLabel(v.billingType),
      price: `${Math.round(v.price ?? 0).toLocaleString('fr-FR')} DA`,
    };
  }
}
