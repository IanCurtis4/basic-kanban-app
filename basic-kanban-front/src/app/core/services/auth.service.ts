import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap, catchError } from 'rxjs';
import { LoginRequest, RegisterRequest, AuthResponse, AuthState } from '../models/auth.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = `${environment.apiUrl}/api/auth`;
  private authState = new BehaviorSubject<AuthState>({
    token: this.getStoredToken(),
    user: this.getStoredUser(),
    isAuthenticated: !!this.getStoredToken(),
    loading: false,
    error: null
  });

  public auth$ = this.authState.asObservable();

  constructor(private http: HttpClient) {}

  register(registerDto: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, registerDto).pipe(
      tap(response => this.handleAuthSuccess(response)),
      catchError(error => {
        this.setError(error.error?.message || 'Registration failed');
        throw error;
      })
    );
  }

  login(loginDto: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, loginDto).pipe(
      tap(response => this.handleAuthSuccess(response)),
      catchError(error => {
        this.setError(error.error?.message || 'Login failed');
        throw error;
      })
    );
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    this.authState.next({
      token: null,
      user: null,
      isAuthenticated: false,
      loading: false,
      error: null
    });
  }

  getToken(): string | null {
    return this.authState.value.token;
  }

  isAuthenticated(): boolean {
    return this.authState.value.isAuthenticated;
  }

  private handleAuthSuccess(response: AuthResponse): void {
    localStorage.setItem('token', response.token);
    localStorage.setItem('user', JSON.stringify(response.user));
    
    this.authState.next({
      token: response.token,
      user: response.user,
      isAuthenticated: true,
      loading: false,
      error: null
    });
  }

  private setError(error: string): void {
    const currentState = this.authState.value;
    this.authState.next({ ...currentState, error, loading: false });
  }

  private getStoredToken(): string | null {
    return localStorage.getItem('token');
  }

  private getStoredUser() {
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
  }
}
