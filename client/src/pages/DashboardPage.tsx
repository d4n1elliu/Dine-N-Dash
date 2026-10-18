import { useAuth } from '../context/useAuth'
import { useSearchParams } from 'react-router-dom'
import MyBookings from '../components/customer-dashboard/MyBookings'
import MyReviews from '../components/customer-dashboard/MyReviews'

type Tab = 'bookings' | 'reviews'

const TABS: { id: Tab; label: string }[] = [
  { id: 'bookings', label: 'Bookings' },
  { id: 'reviews', label: 'My reviews' },
]

function readTab(value: string | null): Tab {
  return TABS.find((tab) => tab.id === value)?.id ?? 'bookings'
}

export default function DashboardPage() {
  const { user } = useAuth()
  const [searchParams, setSearchParams] = useSearchParams()
  const tab = readTab(searchParams.get('tab'))

  return (
    <section className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Dashboard</h1>
        <p className="text-gray-600">Welcome back{user ? `, ${user.name}` : ''}.</p>
      </div>

      <div role="tablist" aria-label="Dashboard sections" className="flex gap-1 border-b border-gray-200">
        {TABS.map(({ id, label }) => (
          <button
            key={id}
            type="button"
            role="tab"
            id={`tab-${id}`}
            aria-selected={tab === id}
            aria-controls={`panel-${id}`}
            onClick={() => setSearchParams({ tab: id })}
            className={`-mb-px border-b-2 px-4 py-2 text-sm font-medium transition ${
              tab === id ? 'border-orange-600 text-orange-700' : 'border-transparent text-gray-600 hover:text-gray-900'
            }`}
          >
            {label}
          </button>
        ))}
      </div>

      <div role="tabpanel" id={`panel-${tab}`} aria-labelledby={`tab-${tab}`}>
        {tab === 'bookings' ? <MyBookings /> : <MyReviews />}
      </div>
    </section>
  )
}
