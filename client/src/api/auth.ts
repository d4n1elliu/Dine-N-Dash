import { api } from './client'

// Must match the UserRole enum in the backend
export type UserRole = 'Diner' | 'RestaurantOwner'

export interface AuthUser {
  id: number
  name: string
  email: string
  role: UserRole
}

export interface LoginResponse {
  token: string
  user: AuthUser
}

export function login(email: string, password: string): Promise<LoginResponse> {
  return api.post<LoginResponse>('/api/auth/login', { email, password })
}
