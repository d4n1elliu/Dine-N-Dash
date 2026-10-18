import { bookingStatus, type Booking, type BookingStatus } from '../../api/bookings'

const statusClasses: Record<BookingStatus, string> = {
  Pending: 'bg-amber-100 text-amber-800',
  Confirmed: 'bg-green-100 text-green-800',
  Declined: 'bg-red-100 text-red-800',
  Cancelled: 'bg-gray-100 text-gray-700',
  Completed: 'bg-blue-100 text-blue-800',
}

export default function StatusBadge({ booking }: { booking: Booking }) {
  const status = bookingStatus(booking)
  return (
    <span className={`rounded-full px-3 py-1 text-xs font-semibold ${statusClasses[status]}`}>{status}</span>
  )
}