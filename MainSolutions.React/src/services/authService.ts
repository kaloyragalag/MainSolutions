import { LoginRequest, LoginResponse, RegisterRequest } from '../types/auth';
import { tokenStorage } from './storage';

const API_URL = process.env.REACT_APP_API_URL || 'https://localhost:5001';

export const authService = {
  async login(request: LoginRequest): Promise<LoginResponse> {
    const response = await fetch(`${API_URL}/api/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(request),
    });
    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || 'Login failed.');
    }
    return response.json();
  },

  async register(request: RegisterRequest): Promise<LoginResponse> {
    const response = await fetch(`${API_URL}/api/auth/register`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(request),
    });
    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || 'Registration failed.');
    }
    return response.json();
  },

  saveSession(data: LoginResponse): void {
    tokenStorage.saveSession(data);
  },

  clearSession(): void {
    tokenStorage.clearSession();
  },

  getToken(): string | null {
    return tokenStorage.getToken();
  },

  getUser(): LoginResponse | null {
    return tokenStorage.getUser<LoginResponse>();
  },

  isAuthenticated(): boolean {
    return !!tokenStorage.getToken();
  },
};
