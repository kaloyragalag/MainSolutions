/**
 * Storage abstraction. Concrete services depend on this interface rather
 * than directly on `localStorage`, so the storage mechanism can be swapped
 * (e.g. sessionStorage, in-memory store for tests) without touching
 * service code — Dependency Inversion Principle.
 */
export interface TokenStorage {
  getToken(): string | null;
  getUser<T>(): T | null;
  saveSession<T extends { token: string }>(data: T): void;
  clearSession(): void;
}

class LocalStorageTokenStorage implements TokenStorage {
  private readonly TOKEN_KEY = 'token';
  private readonly USER_KEY = 'user';

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  getUser<T>(): T | null {
    const raw = localStorage.getItem(this.USER_KEY);
    return raw ? (JSON.parse(raw) as T) : null;
  }

  saveSession<T extends { token: string }>(data: T): void {
    localStorage.setItem(this.TOKEN_KEY, data.token);
    localStorage.setItem(this.USER_KEY, JSON.stringify(data));
  }

  clearSession(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
  }
}

// Single shared instance used across the app. Replace this export with a
// different TokenStorage implementation to change persistence strategy.
export const tokenStorage: TokenStorage = new LocalStorageTokenStorage();
