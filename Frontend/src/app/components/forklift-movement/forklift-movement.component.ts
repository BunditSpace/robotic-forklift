
import { FormsModule } from '@angular/forms';
import { Component, inject, OnInit } from '@angular/core';
import { ForkliftParserService } from '../../services/forklift-parser.service';
import { ForkliftService } from '../../services/forklift.service';
import { Forklift } from '../../shared/models/forklift';

@Component({
  selector: 'app-forklift-movement',
  standalone: true,
  imports: [FormsModule],
  templateUrl: 'forklift-movement.component.html',
  styleUrls: ['forklift-movement.component.css'],
})
export class ForkliftMovementComponent implements OnInit {
  commandString: string = '';
  actions: string[] = [];
  error: string | null = null;

  forklifts: Forklift[] = [];
  selectedForkliftId: string = '';

  get selectedForklift(): Forklift | undefined {
    return this.forklifts.find(f => f.id === this.selectedForkliftId);
  }

  private parseService = inject(ForkliftParserService);
  private forkliftService = inject(ForkliftService);

  ngOnInit(): void {
    this.forkliftService.getAllForklifts().subscribe({
      next: (data) => (this.forklifts = data),
      error: () => (this.error = 'Failed to load forklifts.')
    });
  }

  onForkliftChange(): void {
    this.commandString = '';
    this.actions = [];
    this.error = null;
  }

  parseCommand() {
    if (this.commandString.trim()) {
      const result = this.parseService.parseCommand(this.commandString);
      this.actions = result.actions;
      this.error = result.error;
    }
  }
}
