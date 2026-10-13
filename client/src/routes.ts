export const ROUTES = {
  home: '/',
  login: '/login',
  dashboard: '/dashboard',
} as const

export interface RedirectState {
  from?: string
}
