import { api } from './client'

export type AnalyticsPeriod = 'day' | 'week'

export interface BookingCount {
  start: string // a date like 2026-10-18
  end: string // last date the bar covers 
  bookings: number
  guests: number
}

export function getBookingCounts(period: AnalyticsPeriod): Promise<BookingCount[]> {
  return api.get<BookingCount[]>(`/api/analytics/bookings?period=${period}`)
}