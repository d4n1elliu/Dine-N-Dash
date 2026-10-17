import { useCallback, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { getMenu } from '../api/menu'
import { getMyRestaurant, type RestaurantProfile } from '../api/restaurants'
import BookingsTab from '../components/restaurant-dashboard/BookingsTab'
import MenuTab from '../components/restaurant-dashboard/MenuTab'
import ProfileTab from '../components/restaurant-dashboard/ProfileTab'
import { useApi } from '../hooks/useApi'

type Tab = 'profile' | 'menu' | 'bookings'

const TABS: { id: Tab; label: string }[] = [
  { id: 'profile', label: 'Profile' },
  { id: 'menu', label: 'Menu' },
  { id: 'bookings', label: 'Bookings' },
]

const SETUP_STEPS = ['Restaurant profile', 'Menu']

function readTab(value: string | null): Tab {
  return TABS.find((tab) => tab.id === value)?.id ?? 'bookings'
}

// Owners finish setup before anything else: first the profile, then at least one menu item.
// Until then the matching screen is shown on its own; afterwards the tabs are free to use
export default function RestaurantDashboardPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const profileResult = useApi(getMyRestaurant)
  // Saves from the profile tab replace the loaded profile without fetching it again
  const [savedProfile, setSavedProfile] = useState<RestaurantProfile | null>(null)
  const profile = savedProfile ?? profileResult.data

  const restaurantId = profile?.id
  const fetchMenu = useCallback(
    () => (restaurantId === undefined ? Promise.resolve([]) : getMenu(restaurantId)),
    [restaurantId],
  )
  const menu = useApi(fetchMenu)

  if (profileResult.loading) return <p className="text-gray-500">Loading your restaurant...</p>
  if (profileResult.error) return <p className="text-red-700">{profileResult.error}</p>

  const hasMenuItems = (menu.data ?? []).some((category) => category.items.length > 0)

  if (!profile) {
    return (
      <SetupLayout step={0} title="Set up your restaurant" intro="Tell diners about your restaurant. You can change these details later.">
        <ProfileTab profile={null} onSaved={setSavedProfile} />
      </SetupLayout>
    )
  }

  // Waits for the menu so a restaurant with items does not flash the setup screen
  if (menu.loading && !menu.data) return <p className="text-gray-500">Loading your menu...</p>

  if (!hasMenuItems) {
    return (
      <SetupLayout
        step={1}
        title="Add your menu"
        intro="Create a category, then add at least one item to it. Your dashboard opens once your menu has an item."
      >
        <MenuTab menu={menu} />
      </SetupLayout>
    )
  }

  const tab = readTab(searchParams.get('tab'))

  return (
    <section className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">{profile.name}</h1>
        <p className="text-gray-600">Manage your restaurant, menu and bookings.</p>
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
        {tab === 'profile' && <ProfileTab profile={profile} onSaved={setSavedProfile} />}
        {tab === 'menu' && <MenuTab menu={menu} />}
        {tab === 'bookings' && <BookingsTab />}
      </div>
    </section>
  )
}

interface SetupLayoutProps {
  step: number
  title: string
  intro: string
  children: React.ReactNode
}

function SetupLayout({ step, title, intro, children }: SetupLayoutProps) {
  return (
    <section className="mx-auto max-w-2xl space-y-6">
      <ol className="flex gap-2 text-sm" aria-label="Setup progress">
        {SETUP_STEPS.map((label, index) => (
          <li
            key={label}
            aria-current={index === step ? 'step' : undefined}
            className={`flex-1 rounded border-t-4 pt-2 ${
              index <= step ? 'border-orange-600 font-medium text-gray-900' : 'border-gray-200 text-gray-500'
            }`}
          >
            Step {index + 1}: {label}
          </li>
        ))}
      </ol>
      <div>
        <h1 className="text-3xl font-bold">{title}</h1>
        <p className="text-gray-600">{intro}</p>
      </div>
      {children}
    </section>
  )
}
