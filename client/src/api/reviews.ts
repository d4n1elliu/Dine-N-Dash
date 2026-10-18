import { api } from './client'

export interface Review {
  id: number
  restaurantId: number
  restaurantName: string | null
  dinerName: string | null
  rating: number
  comment: string
  createdAt: string
}

export interface ReviewInput {
  rating: number
  comment: string
}

export function getMyReviews(): Promise<Review[]> {
  return api.get<Review[]>('/api/review/mine')
}

export function getRestaurantReviews(restaurantId: number): Promise<Review[]> {
  return api.get<Review[]>(`/api/review/restaurant/${restaurantId}`)
}

export function getAverageRating(restaurantId: number): Promise<{ average: number }> {
  return api.get<{ average: number }>(`/api/review/restaurant/${restaurantId}/average`)
}

export function createReview(restaurantId: number, input: ReviewInput): Promise<Review> {
  return api.post<Review>('/api/review', { restaurantId, ...input })
}

export function updateReview(id: number, input: ReviewInput): Promise<Review> {
  return api.put<Review>(`/api/review/${id}`, input)
}

export function deleteReview(id: number): Promise<void> {
  return api.delete<void>(`/api/review/${id}`)
}