import { CommonModule, formatDate } from '@angular/common';
import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { Forklift } from '../../shared/models/forklift';
import { ForkliftService } from '../../services/forklift.service';
import { finalize, retry } from 'rxjs';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-forklift-list',
  standalone: true,
  imports: [CommonModule, MatDialogModule],
  templateUrl: './forklift-list.component.html',
  styleUrls: ['./forklift-list.component.css'],
})
export class ForkliftListComponent implements OnInit {
  forklifts: Forklift[] = [];
  searchTerm = '';
  deletingIds = new Set<string>();
  isDeletingAll = false;
  forkliftService = inject(ForkliftService);
  cdr = inject(ChangeDetectorRef);
  dialog = inject(MatDialog);

  get filteredForklifts(): Forklift[] {
    const term = this.searchTerm.trim().toLowerCase();
    if (!term) {
      return this.forklifts;
    }

    return this.forklifts.filter((forklift) => {
      const manufacturingDate = new Date(forklift.manufacturingDate);
      const manufacturingDateText = Number.isNaN(manufacturingDate.getTime())
        ? ''
        : formatDate(manufacturingDate, 'yyyy-MM-dd', 'en-US').toLowerCase();

      return (
        (forklift.name ?? '').toLowerCase().includes(term) ||
        (forklift.modelNumber ?? '').toLowerCase().includes(term) ||
        manufacturingDateText.includes(term)
      );
    });
  }

  ngOnInit(): void {
    this.loadForklifts();
  }

  loadForklifts() {
    this.forkliftService
      .getAllForklifts()
      .pipe(retry({ count: 1, delay: 300 }))
      .subscribe({
        next: (data) => {
          this.forklifts = [...data];
          this.cdr.detectChanges();
        },
        error: (err) => {
          console.error('Error fetching forklifts:', err);
          this.cdr.detectChanges();
        },
      });
  }

  onUploadSuccess(): void {
    this.loadForklifts();
  }

  onSearchInput(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.searchTerm = value;
  }

  deleteAllForklifts(): void {
    if (this.isDeletingAll) return;

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '360px',
      data: {
        title: 'Delete All Forklifts',
        message: 'Are you sure you want to delete all forklifts? This action cannot be undone.',
        confirmText: 'Delete All',
        cancelText: 'Cancel',
      },
    });

    dialogRef.afterClosed().subscribe((isConfirmed) => {
      if (!isConfirmed) return;

      this.isDeletingAll = true;

      this.forkliftService
        .deleteAllForklifts()
        .pipe(finalize(() => {
          this.isDeletingAll = false;
          this.cdr.detectChanges();
        }))
        .subscribe({
          next: () => {
            this.forklifts = [];
            this.cdr.detectChanges();
          },
          error: (err) => {
            console.error('Error deleting all forklifts:', err);
            this.cdr.detectChanges();
          },
        });
    });
  }

  deleteForklift(forklift: Forklift): void {
    const id = forklift.id;
    if (this.deletingIds.has(id)) {
      return;
    }

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '360px',
      data: {
        title: 'Delete Forklift',
        subtitle: `${forklift.name} (${forklift.modelNumber})`,
        message: 'Are you sure you want to delete this forklift?',
        confirmText: 'Delete',
        cancelText: 'Cancel',
      },
    });

    dialogRef.afterClosed().subscribe((isConfirmed) => {
      if (!isConfirmed) {
        return;
      }

      this.deletingIds.add(id);

      this.forkliftService
        .deleteForklift(id)
        .pipe(finalize(() => this.deletingIds.delete(id)))
        .subscribe({
          next: () => {
            this.forklifts = this.forklifts.filter((forklift) => forklift.id !== id);
            this.loadForklifts();
            this.cdr.detectChanges();
          },
          error: (err) => {
            console.error('Error deleting forklift:', err);
            this.cdr.detectChanges();
          },
        });
    });
  }

}
