import type { UserRole } from './api/auth'

export const ROUTES = {
  home: '/',
  search: '/search',
  restaurant: '/restaurants/:id',
  login: '/login',
  register: '/register',
  dashboard: '/dashboard',
  restaurantDashboard: '/restaurant/dashboard',
  bookRestaurant: '/restaurants/:id/book',
} as const

export function restaurantPath(id: number): string {
  return `/restaurants/${id}`
}

export function bookingPath(id: number): string {
  return `/restaurants/${id}/book`
}

// Where a user lands after logging in or registering when no earlier page asked to return to
export function homeFor(role: UserRole | undefined): string {
  return role === 'RestaurantOwner' ? ROUTES.restaurantDashboard : ROUTES.home
}

export function dashboardFor(role: UserRole | undefined): string {
  return role === 'RestaurantOwner' ? ROUTES.restaurantDashboard : ROUTES.dashboard
}

export interface RedirectState {
  from?: string
}
