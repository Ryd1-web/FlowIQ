import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

export interface AuthPayload {
  token?: string;
  Token?: string;
}

export interface OtpPayload {
  message?: string;
  Message?: string;
  phoneNumber?: string;
  PhoneNumber?: string;
}

export interface UpdateProfileRequest {
  fullName?: string;
  phoneNumber?: string;
}

export interface UserProfilePayload {
  id?: string;
  Id?: string;
  fullName?: string;
  FullName?: string;
  phoneNumber?: string;
  PhoneNumber?: string;
  isVerified?: boolean;
  IsVerified?: boolean;
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private apiUrl = environment.apiBaseUrl + '/auth';
  private tokenKey = 'flowiq_jwt';

  constructor(private http: HttpClient) {}

  login(phone: string, otp: string): Observable<AuthPayload> {
    // Backend expects { phoneNumber, otpCode } for /verify-otp
    return this.http
      .post<ApiResponse<AuthPayload> | AuthPayload>(`${this.apiUrl}/verify-otp`, { PhoneNumber: phone, OtpCode: otp })
      .pipe(
      map((res) => this.extractAuthPayload(res)),
      tap(res => this.setToken(res.token || res.Token || '')),
      catchError(this.handleError)
    );
  }

  setToken(token: string) {
    if (!this.isJwt(token)) {
      return;
    }
    localStorage.setItem(this.tokenKey, token);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  logout() {
    localStorage.removeItem(this.tokenKey);
  }

  isLoggedIn(): boolean {
    return this.isJwt(this.getToken());
  }

  // Extract businessId from JWT (assumes businessId is a claim in the token payload)
  getBusinessId(): string | null {
    const token = this.getToken();
    if (!token) return null;
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload["businessId"] || payload["business_id"] || null;
    } catch {
      return null;
    }
  }

  requestOtp(phone: string) {
    // Backend expects { PhoneNumber }
    return this.http.post<any>(`${this.apiUrl}/request-otp`, { PhoneNumber: phone }).pipe(
      catchError(this.handleError)
    );
  }

  signup(phone: string, fullName: string): Observable<ApiResponse<OtpPayload> | OtpPayload> {
    return this.http.post<ApiResponse<OtpPayload> | OtpPayload>(`${this.apiUrl}/register`, {
      PhoneNumber: phone,
      FullName: fullName
    }).pipe(
      catchError(this.handleError)
    );
  }

  updateProfile(request: UpdateProfileRequest): Observable<UserProfilePayload> {
    return this.http
      .put<ApiResponse<UserProfilePayload> | UserProfilePayload>(`${this.apiUrl}/profile`, {
        FullName: request.fullName ?? null,
        PhoneNumber: request.phoneNumber ?? null
      })
      .pipe(
        map((res) => {
          const apiResponse = res as ApiResponse<UserProfilePayload>;
          if (apiResponse && typeof apiResponse === 'object' && 'data' in apiResponse && apiResponse.data) {
            return apiResponse.data;
          }
          return res as UserProfilePayload;
        }),
        catchError(this.handleError)
      );
  }

  getCurrentUserFromToken(): { fullName: string; phoneNumber: string } {
    const token = this.getToken();
    if (!token) {
      return { fullName: '', phoneNumber: '' };
    }

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return {
        fullName: payload.fullName || payload.FullName || payload.name || payload.unique_name || '',
        phoneNumber: payload.phoneNumber || payload.PhoneNumber || payload.phone || ''
      };
    } catch {
      return { fullName: '', phoneNumber: '' };
    }
  }

  private handleError(error: any) {
    return throwError(() => error);
  }

  private extractAuthPayload(response: ApiResponse<AuthPayload> | AuthPayload): AuthPayload {
    const apiResponse = response as ApiResponse<AuthPayload>;
    if (apiResponse && typeof apiResponse === 'object' && 'data' in apiResponse && apiResponse.data) {
      return apiResponse.data;
    }
    return response as AuthPayload;
  }

  private isJwt(token: string | null | undefined): token is string {
    return !!token && token.split('.').length === 3;
  }
}
