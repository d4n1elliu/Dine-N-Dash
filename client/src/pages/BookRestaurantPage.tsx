import type { ChangeEvent, FormEvent } from 'react'
import { useCallback, useState } from 'react'
import { Navigate, useLocation, useParams } from 'react-router-dom'
import { checkAvailability, createBooking, type Booking } from '../api/bookings'
import { getMenu, type MenuCategory } from '../api/menu'
import { getRestaurant } from '../api/restaurants'
import Button from '../components/Button'
import FormError from '../components/FormError'
import TextLink from '../components/TextLink'
import { useAuth } from '../context/useAuth'
import { useApi } from '../hooks/useApi'
import { useSubmit } from '../hooks/useSubmit'
import { formatPrice } from '../lib/format'
import { restaurantPath, ROUTES, type RedirectState } from '../routes'

const MAX_PARTY_SIZE = 20
const MAX_PER_DISH = 10
const inputClass =
  'w-full rounded border border-gray-300 bg-white px-3 py-2 text-sm focus:border-orange-500 focus:outline-none'
const timeFormat = new Intl.DateTimeFormat('en-AU', { dateStyle: 'full', timeStyle: 'short' })

// 11:00 to 21:30 in half hour steps, so two bookings for the same slot always share an exact time
const TIME_SLOTS = Array.from({ length: 22 }, (_, i) => {
  const minutes = 11 * 60 + i * 30
  return `${String(Math.floor(minutes / 60)).padStart(2, '0')}:${String(minutes % 60).padStart(2, '0')}`
})

async function loadBookingData(id: number) {
  const restaurant = await getRestaurant(id)
  const menu = restaurant ? await getMenu(id) : []
  return { restaurant, menu }
}

export default function BookRestaurantPage() {
  const { id } = useParams()
  const restaurantId = Number(id)
  const validId = Number.isInteger(restaurantId) && restaurantId > 0
  const { isAuthenticated, user } = useAuth()
  const location = useLocation()

  const fetchData = useCallback(
    () => (validId ? loadBookingData(restaurantId) : Promise.resolve({ restaurant: null, menu: [] })),
    [validId, restaurantId],
  )
  const result = useApi(fetchData)

  if (!isAuthenticated) {
    const state: RedirectState = { from: location.pathname }
    return <Navigate to={ROUTES.login} state={state} replace />
  }

  if (user?.role !== 'Diner') {
    return (
      <section className="space-y-2 text-center">
        <h1 className="text-3xl font-bold">Bookings are for diner accounts</h1>
        <p className="text-gray-600">Log in with a diner account to book a table.</p>
      </section>
    )
  }

  if (result.loading) return <p className="text-gray-500">Loading...</p>
  if (result.error) return <p className="text-red-700">{result.error}</p>

  const { restaurant, menu } = result.data ?? { restaurant: null, menu: [] }

  if (!restaurant) {
    return (
      <section className="space-y-4 text-center">
        <h1 className="text-3xl font-bold">Restaurant not found</h1>
        <TextLink to={ROUTES.search}>Browse restaurants</TextLink>
      </section>
    )
  }

  return <BookingForm restaurantId={restaurant.id} restaurantName={restaurant.name} menu={menu} />
}

interface BookingFormProps {
  restaurantId: number
  restaurantName: string
  menu: MenuCategory[]
}

function BookingForm({ restaurantId, restaurantName, menu }: BookingFormProps) {
  const [date, setDate] = useState('')
  const [time, setTime] = useState('')
  const [partySize, setPartySize] = useState('2')
  const [notes, setNotes] = useState('')
  const [quantities, setQuantities] = useState<Record<number, number>>({})
  const [booked, setBooked] = useState<Booking | null>(null)
  const { submitting, error, setError, run } = useSubmit('Could not make the booking. Please try again.')

  const size = Number(partySize)
  const sizeValid = Number.isInteger(size) && size >= 1 && size <= MAX_PARTY_SIZE
  const start = toStartTime(date, time)
  const startIso = start && sizeValid ? start.toISOString() : null

  // Asks the API whether the slot has room whenever the date, time or party size changes
  const fetchAvailability = useCallback(
    () => (startIso ? checkAvailability(restaurantId, startIso, size) : Promise.resolve(null)),
    [restaurantId, startIso, size],
  )
  const availability = useApi(fetchAvailability)
  const slotFull = availability.data?.available === false

  const categories = menu.filter((category) => category.items.length > 0)

  // Clears the error on edit so it never describes a value the user has since changed
  const onFieldChange =
    (setValue: (value: string) => void) =>
    (event: ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => {
      setValue(event.target.value)
      setError(null)
    }

  function changeQuantity(itemId: number, delta: number) {
    setQuantities((current) => {
      const next = Math.min(MAX_PER_DISH, Math.max(0, (current[itemId] ?? 0) + delta))
      return { ...current, [itemId]: next }
    })
  }

  function validate(): string | null {
    if (!date) return 'Choose a date.'
    if (!time) return 'Choose a time.'
    if (!sizeValid) return `Party size must be between 1 and ${MAX_PARTY_SIZE}.`
    if (!start || start.getTime() <= Date.now()) return 'Choose a time in the future.'
    if (slotFull) return 'That time is fully booked. Try another time.'
    return null
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    const validationError = validate()
    if (validationError || !start) {
      setError(validationError)
      return
    }

    void run(async () => {
      setBooked(
        await createBooking({
          restaurantId,
          bookingTime: start.toISOString(),
          partySize: size,
          specialRequests: buildSpecialRequests(menu, quantities, notes),
        }),
      )
    })
  }

  if (booked) {
    return (
      <section className="mx-auto max-w-lg space-y-4 text-center">
        <h1 className="text-3xl font-bold">Booking requested</h1>
        <p className="text-gray-800">
          {restaurantName} · {booked.partySize} {booked.partySize === 1 ? 'guest' : 'guests'} ·{' '}
          {timeFormat.format(new Date(booked.bookingTime))}
        </p>
        <p className="text-gray-600">
          The restaurant will confirm your booking. You can follow its status on your dashboard.
        </p>
        <TextLink to={ROUTES.dashboard}>Go to my dashboard</TextLink>
      </section>
    )
  }

  return (
    <section className="mx-auto max-w-2xl space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Book a table</h1>
        <p className="text-gray-600">at {restaurantName}</p>
      </div>

      <form onSubmit={handleSubmit} noValidate className="space-y-5 rounded border border-gray-200 bg-white p-6">
        <div className="grid gap-4 sm:grid-cols-3">
          <label className="space-y-1 text-sm font-medium">
            Date
            <input
              type="date"
              min={toDateInputValue(new Date())}
              value={date}
              onChange={onFieldChange(setDate)}
              className={inputClass}
            />
          </label>
          <label className="space-y-1 text-sm font-medium">
            Time
            <select value={time} onChange={onFieldChange(setTime)} className={inputClass}>
              <option value="">Choose a time</option>
              {TIME_SLOTS.map((slot) => (
                <option key={slot} value={slot}>
                  {slotLabel(slot)}
                </option>
              ))}
            </select>
          </label>
          <label className="space-y-1 text-sm font-medium">
            Guests
            <input
              type="number"
              min={1}
              max={MAX_PARTY_SIZE}
              value={partySize}
              onChange={onFieldChange(setPartySize)}
              className={inputClass}
            />
          </label>
        </div>

        {availability.data?.available && <p className="text-sm text-green-700">A table is available at that time.</p>}
        {slotFull && (
          <p role="alert" className="text-sm text-red-700">
            That time is fully booked for a party of {size}. Try another time.
          </p>
        )}

        {categories.length > 0 && (
          <fieldset className="space-y-3">
            <legend className="text-sm font-medium">Pre-order dishes (optional)</legend>
            {categories.map((category) => (
              <div key={category.id}>
                <h2 className="text-sm font-semibold text-gray-700">{category.name}</h2>
                <ul className="divide-y divide-gray-100">
                  {category.items.map((item) => (
                    <li key={item.id} className="flex items-center gap-3 py-2">
                      <div className="min-w-0 flex-1">
                        <p className="text-sm font-medium">{item.name}</p>
                        <p className="text-xs text-gray-600">{formatPrice(item.price)}</p>
                      </div>
                      <div className="flex items-center gap-2">
                        <button
                          type="button"
                          aria-label={`Remove one ${item.name}`}
                          disabled={(quantities[item.id] ?? 0) === 0}
                          onClick={() => changeQuantity(item.id, -1)}
                          className="rounded border border-gray-300 px-2 py-1 text-sm disabled:opacity-40"
                        >
                          −
                        </button>
                        <span className="w-6 text-center text-sm">{quantities[item.id] ?? 0}</span>
                        <button
                          type="button"
                          aria-label={`Add one ${item.name}`}
                          onClick={() => changeQuantity(item.id, 1)}
                          className="rounded border border-gray-300 px-2 py-1 text-sm"
                        >
                          +
                        </button>
                      </div>
                    </li>
                  ))}
                </ul>
              </div>
            ))}
          </fieldset>
        )}

        <label className="block space-y-1 text-sm font-medium">
          Notes for the restaurant (optional)
          <textarea
            rows={3}
            maxLength={300}
            value={notes}
            onChange={onFieldChange(setNotes)}
            className={inputClass}
            placeholder="Allergies, a window seat, a birthday..."
          />
        </label>

        <FormError message={error} />
        <Button type="submit" fullWidth disabled={submitting || slotFull}>
          {submitting ? 'Booking...' : 'Request booking'}
        </Button>
      </form>

      <TextLink to={restaurantPath(restaurantId)}>Back to restaurant</TextLink>
    </section>
  )
}

function toDateInputValue(date: Date): string {
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')
  return `${date.getFullYear()}-${month}-${day}`
}

// Combines the chosen date and time in the user's own timezone; toISOString then gives the exact moment
function toStartTime(date: string, time: string): Date | null {
  if (!date || !time) return null
  const start = new Date(`${date}T${time}`)
  return Number.isNaN(start.getTime()) ? null : start
}

function slotLabel(slot: string): string {
  const [hours, minutes] = slot.split(':').map(Number)
  return `${hours % 12 || 12}:${String(minutes).padStart(2, '0')} ${hours >= 12 ? 'pm' : 'am'}`
}

// The backend has one free-text field, so pre-ordered dishes go in front of the diner's own notes
function buildSpecialRequests(menu: MenuCategory[], quantities: Record<number, number>, notes: string): string | null {
  const dishes = menu
    .flatMap((category) => category.items)
    .filter((item) => (quantities[item.id] ?? 0) > 0)
    .map((item) => `${quantities[item.id]}x ${item.name}`)

  const parts: string[] = []
  if (dishes.length > 0) parts.push(`Pre-order: ${dishes.join(', ')}.`)
  if (notes.trim()) parts.push(`Notes: ${notes.trim()}`)
  return parts.length > 0 ? parts.join(' ') : null
}