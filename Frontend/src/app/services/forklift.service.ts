import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Forklift } from '../shared/models/forklift';
import { API_ENDPOINTS } from '../core/constants/api.constants';

@Injectable({ providedIn: 'root' })
export class ForkliftService {
  private readonly apiUrl = API_ENDPOINTS.forklift;
  constructor(private http: HttpClient) {}

  /**
   * Retrieves all forklifts from the API.
   * @returns An Observable containing an array of Forklift objects retrieved from the API.
   */
  getAllForklifts(): Observable<Forklift[]> {
    return this.http.get<Forklift[]>(`${this.apiUrl}/getAllForklifts`);
  }

  /**
   * Imports forklift data from a file and sends it to the API for processing.
   * @param file The file to import.
   * @returns An Observable representing the result of the import operation.
   */
  importData(file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(`${this.apiUrl}/import`, formData);
  }

  /**
   * Deletes a forklift by id.
   * @param id The forklift id.
   * @returns An Observable representing the delete operation.
   */
  deleteForklift(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/delete/${id}`);
  }

  /**
   * Deletes all forklifts.
   * @returns An Observable representing the delete all operation.
   */
  deleteAllForklifts(): Observable<any> {
    return this.http.delete(`${this.apiUrl}/deleteAll`);
  }
}
