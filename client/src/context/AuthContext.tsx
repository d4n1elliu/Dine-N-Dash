import { useCallback, useMemo, useState, type ReactNode } from 'react'
import { login as loginRequest } from '../api/auth'
import { AuthContext, type AuthContextValue } from './auth-context'
import { clearSession, readSession, writeSession } from './session-storage'

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState(readSession)

  const login = useCallback(async (email: string, password: string) => {
    const result = await loginRequest(email, password)
    writeSession(result.token, result.user)
    setSession({ token: result.token, user: result.user })
  }, [])

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
      logout,
    }),
    [session, login, logout],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
