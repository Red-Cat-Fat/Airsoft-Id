import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Airsofter, AirsofterGameHistoryItem, AirsofterViolation } from './airsofters.models';

export interface UpdateAirsofterRequest {
  firstName: string;
  lastName: string;
  middleName: string | null;
  kitDescription: string | null;
  airsoftGunsList: string | null;
  teamId: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class AirsoftersService {
  constructor(private readonly http: HttpClient) {}

  getAirsofters(callsign?: string): Observable<Airsofter[]> {
    let params = new HttpParams();

    if (callsign && callsign.trim()) {
      params = params.set('callsign', callsign.trim());
    }

    return this.http.get<Airsofter[]>(`${environment.apiUrl}/airsofters`, { params });
  }

  getAirsofterById(id: string): Observable<Airsofter> {
    return this.http.get<Airsofter>(`${environment.apiUrl}/airsofters/${id}`);
  }

  getAirsofterByCallsign(callsign: string): Observable<Airsofter> {
    return this.http.get<Airsofter>(`${environment.apiUrl}/airsofters/callsign/${encodeURIComponent(callsign)}`);
  }

  updateAirsofter(id: string, request: UpdateAirsofterRequest): Observable<void> {
    return this.http.put<void>(`${environment.apiUrl}/airsofters/${id}`, request);
  }

  getAirsofterGameHistory(id: string): Observable<AirsofterGameHistoryItem[]> {
    return this.http.get<AirsofterGameHistoryItem[]>(`${environment.apiUrl}/airsofters/${id}/game-history`);
  }

  getAirsofterViolations(id: string): Observable<AirsofterViolation[]> {
    return this.http.get<AirsofterViolation[]>(`${environment.apiUrl}/airsofters/${id}/violations`);
  }
}
