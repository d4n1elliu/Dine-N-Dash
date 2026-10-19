import { useCallback, useState } from 'react'
import { getBookingCounts, type AnalyticsPeriod, type BookingCount } from '../../api/analytics'
import { useApi } from '../../hooks/useApi'
import { toDateInputValue } from '../../lib/booking'

const CHART_HEIGHT = 160 // pixels for the tallest bar

const PERIODS: { id: AnalyticsPeriod; label: string; description: string }[] = [
  { id: 'day', label: 'This week', description: 'Bookings for each day of the current week.' },
  { id: 'week', label: 'This month', description: 'Bookings for each week of the current month.' },
]

const dayFormat = new Intl.DateTimeFormat('en-AU', { weekday: 'short', day: 'numeric' }) // Mon 19
const dateFormat = new Intl.DateTimeFormat('en-AU', { day: 'numeric', month: 'short' }) // 19 Oct
const monthFormat = new Intl.DateTimeFormat('en-AU', { month: 'long', year: 'numeric' }) // October 2026

function parseDate(value: string): Date {
  return new Date(`${value}T00:00:00`)
}

// "Mon 19" for a day, "5–11 Oct" for a week (or "1 Oct" when a week is cut down to one day)
function bucketLabel(bucket: BookingCount, period: AnalyticsPeriod): string {
  const start = parseDate(bucket.start)
  if (period === 'day') return dayFormat.format(start)
  if (bucket.start === bucket.end) return dateFormat.format(start)
  return `${start.getDate()}–${dateFormat.format(parseDate(bucket.end))}`
}

// Says what the chart is showing, using the dates the API returned
function caption(data: BookingCount[], period: AnalyticsPeriod): string {
  if (data.length === 0) return ''
  const first = parseDate(data[0].start)
  if (period === 'week') return `${monthFormat.format(first)}, by week`
  return `${dateFormat.format(first)} to ${dateFormat.format(parseDate(data[data.length - 1].start))}`
}

export default function AnalyticsTab() {
  const [period, setPeriod] = useState<AnalyticsPeriod>('day')
  const fetchCounts = useCallback(() => getBookingCounts(period), [period])
  const counts = useApi(fetchCounts)

  const description = PERIODS.find((option) => option.id === period)?.description

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold">Bookings</h2>
          <p className="text-sm text-gray-600">{description} Cancelled and declined bookings are not counted.</p>
        </div>
        <div role="group" aria-label="Chart period" className="flex overflow-hidden rounded border border-gray-300">
          {PERIODS.map(({ id, label }) => (
            <button
              key={id}
              type="button"
              aria-pressed={period === id}
              onClick={() => setPeriod(id)}
              className={`px-4 py-2 text-sm font-medium ${
                period === id ? 'bg-orange-600 text-white' : 'bg-white text-gray-700 hover:bg-gray-50'
              }`}
            >
              {label}
            </button>
          ))}
        </div>
      </div>

      {counts.loading && !counts.data && <p className="text-gray-500">Loading your statistics...</p>}
      {counts.error && !counts.data && <p className="text-red-700">{counts.error}</p>}
      {counts.data && <Summary data={counts.data} period={period} />}
      {counts.data && <BarChart data={counts.data} period={period} />}
    </div>
  )
}

function Summary({ data, period }: { data: BookingCount[]; period: AnalyticsPeriod }) {
  const totalBookings = data.reduce((sum, bucket) => sum + bucket.bookings, 0)
  const totalGuests = data.reduce((sum, bucket) => sum + bucket.guests, 0)
  const busiest = data.reduce((best, bucket) => (bucket.bookings > best.bookings ? bucket : best), data[0])

  return (
    <dl className="grid gap-3 sm:grid-cols-3">
      <Stat label="Bookings" value={String(totalBookings)} />
      <Stat label="Guests" value={String(totalGuests)} />
      <Stat label="Busiest" value={busiest && busiest.bookings > 0 ? bucketLabel(busiest, period) : 'None yet'} />
    </dl>
  )
}

function Stat({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded border border-gray-200 bg-white p-4">
      <dt className="text-sm text-gray-600">{label}</dt>
      <dd className="text-2xl font-bold">{value}</dd>
    </div>
  )
}

function BarChart({ data, period }: { data: BookingCount[]; period: AnalyticsPeriod }) {
  const total = data.reduce((sum, bucket) => sum + bucket.bookings, 0)
  if (total === 0) return <p className="text-gray-600">No bookings in this period yet.</p>

  const tallest = Math.max(1, ...data.map((bucket) => bucket.bookings))
  const today = toDateInputValue(new Date())

  return (
    <div className="overflow-x-auto rounded border border-gray-200 bg-white p-4">
      <p className="mb-4 text-center text-sm font-medium text-gray-700">{caption(data, period)}</p>
      <ul className="mx-auto flex w-fit items-end gap-3" aria-label="Bookings chart">
        {data.map((bucket) => {
          const isCurrent = today >= bucket.start && today <= bucket.end
          const label = bucketLabel(bucket, period)

          return (
            <li
              key={bucket.start}
              aria-current={isCurrent ? 'date' : undefined}
              className="flex w-16 flex-col items-center justify-end gap-1"
              title={`${label}: ${bucket.bookings} bookings, ${bucket.guests} guests`}
            >
              <span className="text-xs font-medium text-gray-700">{bucket.bookings}</span>
              <div
                aria-hidden="true"
                className={`w-full rounded-t ${isCurrent ? 'bg-orange-600' : 'bg-orange-300'}`}
                style={{ height: Math.max(2, (bucket.bookings / tallest) * CHART_HEIGHT) }}
              />
              <span className={`text-center text-xs ${isCurrent ? 'font-semibold text-gray-900' : 'text-gray-600'}`}>
                {label}
              </span>
            </li>
          )
        })}
      </ul>
    </div>
  )
}