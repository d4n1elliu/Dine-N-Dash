import { createContext } from 'react'
import type { AuthUser, UserRole } from '../api/auth'

export interface AuthContextValue {
  user: AuthUser | null
  token: string | null
  isAuthenticated: boolean
  // Both resolve to the signed in user so callers can redirect by role
  login: (email: string, password: string) => Promise<AuthUser>
  register: (name: string, email: string, password: string, role: UserRole) => Promise<AuthUser>
  logout: () => void
}

export const AuthContext = createContext<AuthContextValue | undefined>(undefined)
