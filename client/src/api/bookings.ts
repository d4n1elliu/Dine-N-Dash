import { api } from './client'

// Must match the order of the BookingStatus enum in the backend, which is sent as a number
export const BOOKING_STATUSES = ['Pending', 'Confirmed', 'Declined', 'Cancelled', 'Completed'] as const

export type BookingStatus = (typeof BOOKING_STATUSES)[number]

export interface Booking {
  id: number
  restaurantId: number
  restaurantName: string | null
  dinerName: string | null
  bookingTime: string
  partySize: number
  status: number
  specialRequests: string | null
}

export function bookingStatus(booking: Booking): BookingStatus {
  return BOOKING_STATUSES[booking.status] ?? 'Pending'
}

export function getRestaurantBookings(): Promise<Booking[]> {
  return api.get<Booking[]>('/api/booking/restaurant')
}

export function acceptBooking(id: number): Promise<Booking> {
  return api.post<Booking>(`/api/booking/${id}/accept`)
}

export function declineBooking(id: number): Promise<Booking> {
  return api.post<Booking>(`/api/booking/${id}/decline`)
}
