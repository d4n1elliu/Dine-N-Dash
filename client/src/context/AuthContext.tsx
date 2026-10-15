import { useCallback, useMemo, useRef, useState, type ReactNode } from 'react'
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
  // Each login, register or logout bumps this so a slow response from an earlier
  // attempt cannot overwrite a session the user has since replaced
  const attemptRef = useRef(0)

  const runAttempt = useCallback(async (request: () => Promise<AuthResponse>) => {
    const attempt = ++attemptRef.current
    const result = await request()
    if (attempt !== attemptRef.current) {
      throw new Error('This sign in attempt was superseded by a newer one.')
    }
    writeSession(result.token, result.user)
    setSession({ token: result.token, user: result.user })
  }, [])

  const login = useCallback(
    (email: string, password: string) => runAttempt(() => loginRequest(email, password)),
    [runAttempt],
  )

  const register = useCallback(
    (name: string, email: string, password: string, role: UserRole) =>
      runAttempt(() => registerRequest(name, email, password, role)),
    [runAttempt],
  )

  const logout = useCallback(() => {
    attemptRef.current++
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
