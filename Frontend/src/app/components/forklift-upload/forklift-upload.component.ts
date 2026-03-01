
import { ChangeDetectorRef, Component, EventEmitter, inject, Output } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { finalize } from 'rxjs';
import { Router } from '@angular/router';
import { ForkliftService } from '../../services/forklift.service';

@Component({
  selector: 'app-forklift-upload',
  standalone: true,
  imports: [],
  templateUrl: './forklift-upload.component.html',
  styleUrls: ['./forklift-upload.component.css'],
})
export class ForkliftUploadComponent {
  @Output() uploadSuccess = new EventEmitter<void>();

  selectedFile: File | null = null;
  message: string = '';
  isUploading: boolean = false;

  forkliftService = inject(ForkliftService);
  cdr = inject(ChangeDetectorRef);
  router = inject(Router);

  onFileSelected(event: any) {
    this.selectedFile = event.target.files[0];
  }

  clearMessage(): void {
    this.message = '';
    this.cdr.detectChanges();
  }

  onUpload() {
    if (!this.selectedFile || this.isUploading) {
      return;
    }

    this.message = '';
    this.isUploading = true;

    this.forkliftService
      .importData(this.selectedFile)
      .pipe(
        finalize(() => {
          this.isUploading = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: () => {
          this.uploadSuccess.emit();
          this.message = 'File uploaded successfully! Redirecting to list...';
          this.cdr.detectChanges();
          setTimeout(() => this.router.navigate(['/list']), 1500);
        },
        error: (err: HttpErrorResponse) => {
          const backendMessage = err.error?.error ?? err.error?.message;
          this.message = `Error uploading file: ${backendMessage || err.message}`;
          this.cdr.detectChanges();
        },
      });
  }
}
