export const ROUTES = {
  home: '/',
  login: '/login',
  register: '/register',
  dashboard: '/dashboard',
} as const

export interface RedirectState {
  from?: string
}
