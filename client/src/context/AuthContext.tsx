import { useCallback, useMemo, useState, type ReactNode } from 'react'
import {
  login as loginRequest,
  register as registerRequest,
  type AuthResponse,
  type UserRole,
} from '../api/auth'
import { AuthContext, type AuthContextValue } from './auth-context'
import { clearSession, readSession, writeSession } from './session-storage'

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState(readSession)

  const startSession = useCallback((result: AuthResponse) => {
    writeSession(result.token, result.user)
    setSession({ token: result.token, user: result.user })
  }, [])

  const login = useCallback(
    async (email: string, password: string) => {
      startSession(await loginRequest(email, password))
    },
    [startSession],
  )

  const register = useCallback(
    async (name: string, email: string, password: string, role: UserRole) => {
      startSession(await registerRequest(name, email, password, role))
    },
    [startSession],
  )

  const logout = useCallback(() => {
    clearSession()
    setSession({ token: null, user: null })
  }, [])

  const value = useMemo<AuthContextValue>(
    () => ({
      user: session.user,
      token: session.token,
      isAuthenticated: session.token !== null,
      login,
      register,
      logout,
    }),
    [session, login, register, logout],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
