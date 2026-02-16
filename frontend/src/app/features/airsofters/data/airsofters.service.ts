import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Airsofter } from './airsofters.models';

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
}
