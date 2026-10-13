import type { AuthUser } from '../api/auth'

const TOKEN_KEY = 'dnd_token'
const USER_KEY = 'dnd_user'

export interface StoredSession {
  token: string | null
  user: AuthUser | null
}

const emptySession: StoredSession = { token: null, user: null }

export function getStoredToken(): string | null {
  try {
    return localStorage.getItem(TOKEN_KEY)
  } catch {
    return null
  }
}

export function readSession(): StoredSession {
  try {
    const token = localStorage.getItem(TOKEN_KEY)
    const rawUser = localStorage.getItem(USER_KEY)
    if (!token || !rawUser) return emptySession
    return { token, user: JSON.parse(rawUser) as AuthUser }
  } catch {
    return emptySession
  }
}

export function writeSession(token: string, user: AuthUser): void {
  try {
    localStorage.setItem(TOKEN_KEY, token)
    localStorage.setItem(USER_KEY, JSON.stringify(user))
  } catch {
    // Storage unavailable
  }
}

export function clearSession(): void {
  try {
    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(USER_KEY)
  } catch {
    // Storage unavailable
  }
}
