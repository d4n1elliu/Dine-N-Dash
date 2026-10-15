import { api } from './client'

// Must match the UserRole enum in the backend
export type UserRole = 'Diner' | 'RestaurantOwner'

export interface AuthUser {
  id: number
  name: string
  email: string
  role: UserRole
}

export interface AuthResponse {
  token: string
  user: AuthUser
}

export function login(email: string, password: string): Promise<AuthResponse> {
  return api.post<AuthResponse>('/api/auth/login', { email, password })
}

export function register(
  name: string,
  email: string,
  password: string,
  role: UserRole,
): Promise<AuthResponse> {
  return api.post<AuthResponse>('/api/auth/register', { name, email, password, role })
}
