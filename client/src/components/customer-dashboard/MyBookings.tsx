import { useState } from 'react'
import { bookingStatus, cancelBooking, getMyBookings, type Booking } from '../../api/bookings'
import { useApi } from '../../hooks/useApi'
import { useSubmit } from '../../hooks/useSubmit'
import { dangerButtonClass } from '../../lib/form-styles'
import { restaurantPath, ROUTES } from '../../routes'
import FormError from '../FormError'
import TextLink from '../TextLink'
import StatusBadge from './StatusBadge'

const timeFormat = new Intl.DateTimeFormat('en-AU', { dateStyle: 'medium', timeStyle: 'short' })
const secondaryButtonClass = 'rounded border border-gray-300 px-3 py-1 text-sm hover:bg-gray-50'

// Splits the list when it arrives, since checking the clock while rendering would give unstable results
async function loadMyBookings(): Promise<{ upcoming: Booking[]; past: Booking[] }> {
  const all = await getMyBookings()
  const now = Date.now()
  const isUpcoming = (b: Booking) => {
    const status = bookingStatus(b)
    return (status === 'Pending' || status === 'Confirmed') && new Date(b.bookingTime).getTime() > now
  }
  return { upcoming: all.filter(isUpcoming), past: all.filter((b) => !isUpcoming(b)).reverse() }
}

export default function MyBookings() {
  const bookings = useApi(loadMyBookings)

  if (bookings.loading && !bookings.data) return <p className="text-gray-500">Loading your bookings...</p>
  if (bookings.error && !bookings.data) return <p className="text-red-700">{bookings.error}</p>

  const { upcoming, past } = bookings.data ?? { upcoming: [], past: [] }

  return (
    <div className="space-y-8">
      <section className="space-y-3">
        <h2 className="text-lg font-semibold">Upcoming bookings ({upcoming.length})</h2>
        {upcoming.length === 0 ? (
          <p className="text-gray-600">
            You have no upcoming bookings. <TextLink to={ROUTES.search}>Find a restaurant</TextLink>
          </p>
        ) : (
          <ul className="space-y-3">
            {upcoming.map((booking) => (
              <BookingCard key={booking.id} booking={booking} onCancelled={bookings.reload} />
            ))}
          </ul>
        )}
      </section>

      {past.length > 0 && (
        <section className="space-y-3">
          <h2 className="text-lg font-semibold">Past and cancelled bookings</h2>
          <ul className="space-y-3">
            {past.map((booking) => (
              <BookingCard key={booking.id} booking={booking} />
            ))}
          </ul>
        </section>
      )}
    </div>
  )
}

// A card with a cancel button when onCancelled is given; the button asks for a second click before cancelling
function BookingCard({ booking, onCancelled }: { booking: Booking; onCancelled?: () => void }) {
  const [confirming, setConfirming] = useState(false)
  const { submitting, error, run } = useSubmit('Could not cancel the booking. Please try again.')

  function cancel() {
    void run(async () => {
      await cancelBooking(booking.id)
      onCancelled?.()
    })
  }

  return (
    <li className="space-y-2 rounded border border-gray-200 bg-white p-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <p className="font-medium">
            <TextLink to={restaurantPath(booking.restaurantId)}>{booking.restaurantName ?? 'Restaurant'}</TextLink>
          </p>
          <p className="text-sm text-gray-600">
            {timeFormat.format(new Date(booking.bookingTime))} · {booking.partySize}{' '}
            {booking.partySize === 1 ? 'guest' : 'guests'}
          </p>
        </div>
        <div className="flex items-center gap-3">
          <StatusBadge booking={booking} />
          {onCancelled &&
            (confirming ? (
              <div className="flex gap-2">
                <button type="button" className={dangerButtonClass} disabled={submitting} onClick={cancel}>
                  {submitting ? 'Cancelling...' : 'Confirm cancel'}
                </button>
                <button type="button" className={secondaryButtonClass} disabled={submitting} onClick={() => setConfirming(false)}>
                  Never mind
                </button>
              </div>
            ) : (
              <button type="button" className={secondaryButtonClass} onClick={() => setConfirming(true)}>
                Cancel booking
              </button>
            ))}
        </div>
      </div>
      {booking.specialRequests && <p className="text-sm text-gray-700">Request: {booking.specialRequests}</p>}
      <FormError message={error} />
    </li>
  )
}