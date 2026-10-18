import type { Booking } from '../../api/bookings'
import { ROUTES } from '../../routes'
import TextLink from '../TextLink'

const timeFormat = new Intl.DateTimeFormat('en-AU', { dateStyle: 'full', timeStyle: 'short' })

interface BookingConfirmationProps {
  restaurantName: string
  booking: Booking
}

export default function BookingConfirmation({ restaurantName, booking }: BookingConfirmationProps) {
  return (
    <section className="mx-auto max-w-lg space-y-4 text-center">
      <h1 className="text-3xl font-bold">Booking requested</h1>
      <p className="text-gray-800">
        {restaurantName} · {booking.partySize} {booking.partySize === 1 ? 'guest' : 'guests'} ·{' '}
        {timeFormat.format(new Date(booking.bookingTime))}
      </p>
      <p className="text-gray-600">
        The restaurant will confirm your booking. You can follow its status on your dashboard.
      </p>
      <TextLink to={ROUTES.dashboard}>Go to my dashboard</TextLink>
    </section>
  )
}