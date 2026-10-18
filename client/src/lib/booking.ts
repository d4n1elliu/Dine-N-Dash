import type { MenuCategory } from '../api/menu'

// Keep in sync with Booking.Validate() in the backend
export const MAX_PARTY_SIZE = 20
export const MAX_PER_DISH = 10

// 11:00 to 21:30 in 30 min intervals 
export const TIME_SLOTS = Array.from({ length: 22 }, (_, i) => {
  const minutes = 11 * 60 + i * 30
  return `${String(Math.floor(minutes / 60)).padStart(2, '0')}:${String(minutes % 60).padStart(2, '0')}`
})

export function toDateInputValue(date: Date): string {
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')
  return `${date.getFullYear()}-${month}-${day}`
}

// Combines the chosen date and time in the user's own timezone; toISOString then gives the exact moment
export function toStartTime(date: string, time: string): Date | null {
  if (!date || !time) return null
  const start = new Date(`${date}T${time}`)
  return Number.isNaN(start.getTime()) ? null : start
}

export function slotLabel(slot: string): string {
  const [hours, minutes] = slot.split(':').map(Number)
  return `${hours % 12 || 12}:${String(minutes).padStart(2, '0')} ${hours >= 12 ? 'pm' : 'am'}`
}

// The backend has one free-text field, so pre-ordered dishes go in front of the diner's own notes
export function buildSpecialRequests(
  menu: MenuCategory[],
  quantities: Record<number, number>,
  notes: string,
): string | null {
  const dishes = menu
    .flatMap((category) => category.items)
    .filter((item) => (quantities[item.id] ?? 0) > 0)
    .map((item) => `${quantities[item.id]}x ${item.name}`)

  const parts: string[] = []
  if (dishes.length > 0) parts.push(`Pre-order: ${dishes.join(', ')}.`)
  if (notes.trim()) parts.push(`Notes: ${notes.trim()}`)
  return parts.length > 0 ? parts.join(' ') : null
}