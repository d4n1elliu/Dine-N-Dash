import { api, ApiError } from './client'

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
  profileImageUrl: string | null
}

// The owner's view of their own restaurant
export interface RestaurantProfile extends Restaurant {
  capacity: number
}

export interface RestaurantProfileInput {
  name: string
  description: string
  cuisine: number
  address: string
  suburb: string
  priceLevel: number
  capacity: number
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

// Resolves to null when no restaurant has that id
export async function getRestaurant(id: number): Promise<Restaurant | null> {
  try {
    return await api.get<Restaurant>(`/api/restaurant/${id}`)
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) return null
    throw error
  }
}

// Resolves to null when the owner has not created their restaurant yet
export async function getMyRestaurant(): Promise<RestaurantProfile | null> {
  try {
    return await api.get<RestaurantProfile>('/api/restaurant/mine')
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) return null
    throw error
  }
}

export function createMyRestaurant(input: RestaurantProfileInput): Promise<RestaurantProfile> {
  return api.post<RestaurantProfile>('/api/restaurant/mine', input)
}

export function updateMyRestaurant(input: RestaurantProfileInput): Promise<RestaurantProfile> {
  return api.put<RestaurantProfile>('/api/restaurant/mine', input)
}

export function uploadRestaurantImage(file: File): Promise<RestaurantProfile> {
  return api.upload<RestaurantProfile>('/api/restaurant/mine/image', file)
}

export function removeRestaurantImage(): Promise<RestaurantProfile> {
  return api.delete<RestaurantProfile>('/api/restaurant/mine/image')
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
