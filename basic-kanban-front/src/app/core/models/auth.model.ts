export interface RegisterRequest {
  email: string;
  password: string;
  fullName: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  refreshToken?: string;
  user: {
    id: string;
    email: string;
    fullName: string;
  };
}

export interface AuthState {
  token: string | null;
  user: { id: string; email: string; fullName: string } | null;
  isAuthenticated: boolean;
  loading: boolean;
  error: string | null;
}
