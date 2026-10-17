export const ROUTES = {
  home: '/',
  search: '/search',
  login: '/login',
  register: '/register',
  dashboard: '/dashboard',
} as const

export interface RedirectState {
  from?: string
}
