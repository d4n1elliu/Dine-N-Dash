import { useState } from 'react'
import {
  acceptBooking,
  bookingStatus,
  declineBooking,
  getRestaurantBookings,
  type Booking,
  type BookingStatus,
} from '../../api/bookings'
import { useApi } from '../../hooks/useApi'
import { useSubmit } from '../../hooks/useSubmit'
import { dangerButtonClass, smallPrimaryButtonClass } from '../../lib/form-styles'
import FormError from '../FormError'

const timeFormat = new Intl.DateTimeFormat('en-AU', { dateStyle: 'medium', timeStyle: 'short' })

const statusClasses: Record<BookingStatus, string> = {
  Pending: 'bg-amber-100 text-amber-800',
  Confirmed: 'bg-green-100 text-green-800',
  Declined: 'bg-red-100 text-red-800',
  Cancelled: 'bg-gray-100 text-gray-700',
  Completed: 'bg-blue-100 text-blue-800',
}

// Splits the list when it arrives, since checking the clock while rendering would give unstable results
async function loadBookings(): Promise<{ awaiting: Booking[]; others: Booking[] }> {
  const all = await getRestaurantBookings()
  const now = Date.now()
  const isAwaiting = (b: Booking) => bookingStatus(b) === 'Pending' && new Date(b.bookingTime).getTime() > now
  return { awaiting: all.filter(isAwaiting), others: all.filter((b) => !isAwaiting(b)).reverse() }
}

// Pending upcoming bookings come first so the owner can respond to them; everything else is listed below
export default function BookingsTab() {
  const bookings = useApi(loadBookings)

  if (bookings.loading && !bookings.data) return <p className="text-gray-500">Loading bookings...</p>
  if (bookings.error && !bookings.data) return <p className="text-red-700">{bookings.error}</p>

  const { awaiting, others } = bookings.data ?? { awaiting: [], others: [] }

  return (
    <div className="space-y-8">
      <section className="space-y-3">
        <h2 className="text-lg font-semibold">Awaiting your response ({awaiting.length})</h2>
        {awaiting.length === 0 ? (
          <p className="text-gray-600">No bookings need a response right now.</p>
        ) : (
          <ul className="space-y-3">
            {awaiting.map((booking) => (
              <BookingRow key={booking.id} booking={booking} onResponded={bookings.reload} />
            ))}
          </ul>
        )}
      </section>

      {others.length > 0 && (
        <section className="space-y-3">
          <h2 className="text-lg font-semibold">Other bookings</h2>
          <ul className="space-y-3">
            {others.map((booking) => (
              <BookingRow key={booking.id} booking={booking} />
            ))}
          </ul>
        </section>
      )}
    </div>
  )
}

function BookingRow({ booking, onResponded }: { booking: Booking; onResponded?: () => void }) {
  const [respondingTo, setRespondingTo] = useState<'accept' | 'decline' | null>(null)
  const { submitting, error, run } = useSubmit('Could not update the booking. Please try again.')
  const status = bookingStatus(booking)

  function respond(action: 'accept' | 'decline') {
    setRespondingTo(action)
    void run(async () => {
      await (action === 'accept' ? acceptBooking(booking.id) : declineBooking(booking.id))
      onResponded?.()
    })
  }

  return (
    <li className="space-y-2 rounded border border-gray-200 bg-white p-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <p className="font-medium">
            {booking.dinerName ?? 'Diner'} · {booking.partySize} {booking.partySize === 1 ? 'guest' : 'guests'}
          </p>
          <p className="text-sm text-gray-600">{timeFormat.format(new Date(booking.bookingTime))}</p>
        </div>
        {onResponded ? (
          <div className="flex gap-2">
            <button type="button" className={smallPrimaryButtonClass} disabled={submitting} onClick={() => respond('accept')}>
              {submitting && respondingTo === 'accept' ? 'Accepting...' : 'Accept'}
            </button>
            <button type="button" className={dangerButtonClass} disabled={submitting} onClick={() => respond('decline')}>
              {submitting && respondingTo === 'decline' ? 'Declining...' : 'Decline'}
            </button>
          </div>
        ) : (
          <span className={`rounded-full px-3 py-1 text-xs font-semibold ${statusClasses[status]}`}>{status}</span>
        )}
      </div>
      {booking.specialRequests && <p className="text-sm text-gray-700">Request: {booking.specialRequests}</p>}
      <FormError message={error} />
    </li>
  )
}
