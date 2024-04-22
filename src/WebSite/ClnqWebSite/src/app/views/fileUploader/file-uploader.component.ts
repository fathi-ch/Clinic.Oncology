import { Component, EventEmitter, OnInit, Output, ViewChild } from '@angular/core';
import { DxFileUploaderComponent } from 'devextreme-angular';
import notify from 'devextreme/ui/notify';
import { PieceJointe } from 'src/app/models/rdv/piecejointe';


@Component({
  selector: 'app-file-uploader',
  templateUrl: './file-uploader.component.html',
  styleUrls: ['./file-uploader.component.scss']
})
export class FileUploaderComponent implements OnInit {
  @ViewChild('fileUploader', { static: false }) fileUploader: DxFileUploaderComponent | undefined;
  @Output() pjList = new EventEmitter<PieceJointe>();
  assPJComment = [];
  constructor() {
  }

  ngOnInit() { }

  filesUploaded(e: { value: any[]; }) {
    this.assPJComment=[];
    if (e.value.length) {
        e.value.forEach((file) => {
          if (file.size >= 5000000) {
            notify('{' + file.name + '} : La taille de la pièce jointe dépasse la limite maximale autorisée (5 Mo)', 'error', 5000);
          } else {
            let reader = new FileReader();
            reader.onload = (fr: any) => {
              let img = new Image();
              img.src = reader.result?reader.result.toString():"";
              setTimeout(() => {
                let currPJComment: PieceJointe = {
                  fileName: encodeURI(file.name),
                 // patientDocumentsbase64: file.type.indexOf('image') >= 0 ? img.src : ""
                };
                this.pjList.emit(currPJComment);
  
              }, 0);
            };
            reader.readAsDataURL(file);
          }
        });
        if(this.fileUploader)
          this.fileUploader.instance.reset();
          
      
    }
  }


}
