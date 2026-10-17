import type { UserRole } from './api/auth'

export const ROUTES = {
  home: '/',
  search: '/search',
  restaurant: '/restaurants/:id',
  login: '/login',
  register: '/register',
  dashboard: '/dashboard',
  restaurantDashboard: '/restaurant/dashboard',
} as const

export function restaurantPath(id: number): string {
  return `/restaurants/${id}`
}

export function dashboardFor(role: UserRole | undefined): string {
  return role === 'RestaurantOwner' ? ROUTES.restaurantDashboard : ROUTES.dashboard
}

export interface RedirectState {
  from?: string
}
