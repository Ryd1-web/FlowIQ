import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ApiResponse, unwrapApiData } from '../models/api-response.model';

export interface Business {
  id: string;
  name: string;
  description?: string;
  category?: string;
  address?: string;
}

export interface CreateBusinessRequest {
  name: string;
  description?: string;
  category?: string;
  address?: string;
}

@Injectable({ providedIn: 'root' })
export class BusinessService {
  private apiUrl = environment.apiBaseUrl + '/business';

  constructor(private http: HttpClient) {}

  getAllForCurrentUser(): Observable<Business[]> {
    return this.http.get<ApiResponse<any[]> | any[]>(this.apiUrl).pipe(
      map((response) => {
        const businesses = unwrapApiData<any[]>(response, []);
        return businesses
          .filter((x) => !!x)
          .map((b) => ({
            id: b.id ?? b.Id ?? '',
            name: b.name ?? b.Name ?? '',
            description: b.description ?? b.Description ?? '',
            category: b.category ?? b.Category ?? '',
            address: b.address ?? b.Address ?? ''
          }))
          .filter((b) => !!b.id);
      })
    );
  }

  create(request: CreateBusinessRequest): Observable<Business> {
    return this.http.post<ApiResponse<any> | any>(this.apiUrl, {
      Name: request.name,
      Description: request.description || null,
      Category: request.category || null,
      Address: request.address || null
    }).pipe(
      map((response) => {
        const b = unwrapApiData<any>(response, {});
        return {
          id: b.id ?? b.Id ?? '',
          name: b.name ?? b.Name ?? '',
          description: b.description ?? b.Description ?? '',
          category: b.category ?? b.Category ?? '',
          address: b.address ?? b.Address ?? ''
        };
      })
    );
  }
}
