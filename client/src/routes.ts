import type { UserRole } from './api/auth'

export const ROUTES = {
  home: '/',
  search: '/search',
  login: '/login',
  register: '/register',
  dashboard: '/dashboard',
  restaurantDashboard: '/restaurant/dashboard',
} as const

export function dashboardFor(role: UserRole | undefined): string {
  return role === 'RestaurantOwner' ? ROUTES.restaurantDashboard : ROUTES.dashboard
}

export interface RedirectState {
  from?: string
}
