import { api } from './client'

// Must match the order of the CuisineType enum in the backend, which is sent as a number
export const CUISINES = [
  'Australian',
  'Chinese',
  'Japanese',
  'Korean',
  'Thai',
  'Vietnamese',
  'Indian',
  'Italian',
  'Mexican',
  'MiddleEastern',
  'Other',
] as const

export type Cuisine = (typeof CUISINES)[number]

export type RestaurantSort = 'name' | 'price' | 'rating'

export interface Restaurant {
  id: number
  name: string
  description: string
  cuisine: number
  address: string
  suburb: string
  priceLevel: number
  photoUrls: string[]
}

export interface PagedResponse<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export interface RestaurantSearch {
  name?: string
  cuisine?: Cuisine
  suburb?: string
  sortBy?: RestaurantSort
  page?: number
  pageSize?: number
}

export function getRecommendedRestaurants(page = 1, pageSize = 9): Promise<PagedResponse<Restaurant>> {
  return api.get<PagedResponse<Restaurant>>(`/api/restaurant/recommended?${toQuery({ page, pageSize })}`)
}

export function searchRestaurants(search: RestaurantSearch): Promise<PagedResponse<Restaurant>> {
  return api.get<PagedResponse<Restaurant>>(`/api/restaurant?${toQuery({ ...search })}`)
}

// Splits MiddleEastern into Middle Eastern, like ToDisplayName in the backend
export function cuisineLabel(cuisine: number | Cuisine): string {
  const name = typeof cuisine === 'number' ? (CUISINES[cuisine] ?? 'Other') : cuisine
  return name.replace(/([a-z])([A-Z])/g, '$1 $2')
}

function toQuery(params: Record<string, string | number | undefined>): string {
  const query = new URLSearchParams()
  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined && value !== '') query.set(key, String(value))
  }
  return query.toString()
}
