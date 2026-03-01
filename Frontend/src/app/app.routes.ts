import { Routes } from '@angular/router';
import { ForkliftListComponent } from './components/forklift-list/forklift-list.component';
import { ForkliftMovementComponent } from './components/forklift-movement/forklift-movement.component';
import { ForkliftUploadComponent } from './components/forklift-upload/forklift-upload.component';

export const routes: Routes = [
  { path: '', redirectTo: '/import', pathMatch: 'full' },
  { path: 'import', component: ForkliftUploadComponent },
  { path: 'list', component: ForkliftListComponent },
  { path: 'command', component: ForkliftMovementComponent },
];
