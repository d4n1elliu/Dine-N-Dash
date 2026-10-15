import type { UserRole } from '../api/auth'

export interface RoleOption {
  value: UserRole
  label: string
  description: string
}

export const ROLE_OPTIONS: RoleOption[] = [
  { value: 'Diner', label: 'Diner', description: 'Find and book restaurants' },
  { value: 'RestaurantOwner', label: 'Restaurant owner', description: 'List and manage your restaurant' },
]

export const ROLE_BENEFITS: Record<UserRole, string[]> = {
  Diner: [
    'Search restaurants by cuisine, location and rating',
    'Book a table in seconds, no phone call needed',
    'Leave reviews and keep track of your bookings',
  ],
  RestaurantOwner: [
    'List your restaurant and reach new diners',
    'Manage bookings from one dashboard',
    'Respond to reviews and build your reputation',
  ],
}

export function roleLabel(role: UserRole): string {
  return ROLE_OPTIONS.find((option) => option.value === role)?.label ?? role
}
