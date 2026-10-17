import { useCallback } from 'react'
import { useLocation, useNavigate, useParams } from 'react-router-dom'
import { apiUrl } from '../api/client'
import { getMenu, type MenuCategory, type MenuItem } from '../api/menu'
import { cuisineLabel, getRestaurant } from '../api/restaurants'
import Button from '../components/Button'
import TextLink from '../components/TextLink'
import { useAuth } from '../context/useAuth'
import { useApi } from '../hooks/useApi'
import { formatPrice } from '../lib/format'
import { ROUTES, type RedirectState } from '../routes'

// Loads the restaurant and its menu together; a missing restaurant resolves to null rather than an error
async function loadRestaurant(id: number) {
  const restaurant = await getRestaurant(id)
  const menu = restaurant ? await getMenu(id) : []
  return { restaurant, menu }
}

export default function RestaurantPage() {
  const { id } = useParams()
  const restaurantId = Number(id)
  const validId = Number.isInteger(restaurantId) && restaurantId > 0

  const fetchRestaurant = useCallback(
    () => (validId ? loadRestaurant(restaurantId) : Promise.resolve({ restaurant: null, menu: [] })),
    [validId, restaurantId],
  )
  const result = useApi(fetchRestaurant)

  if (result.loading) return <p className="text-gray-500">Loading restaurant...</p>
  if (result.error) return <p className="text-red-700">{result.error}</p>

  const { restaurant, menu } = result.data ?? { restaurant: null, menu: [] }

  if (!restaurant) {
    return (
      <section className="space-y-4 text-center">
        <h1 className="text-3xl font-bold">Restaurant not found</h1>
        <p className="text-gray-600">That restaurant does not exist or has been removed.</p>
        <TextLink to={ROUTES.search}>Browse restaurants</TextLink>
      </section>
    )
  }

  const photo = restaurant.profileImageUrl ? apiUrl(restaurant.profileImageUrl) : null

  return (
    <article className="space-y-8">
      <div className="overflow-hidden rounded border border-gray-200 bg-white">
        {photo ? (
          <img src={photo} alt={restaurant.name} className="h-64 w-full object-cover sm:h-80" />
        ) : (
          <div
            aria-hidden="true"
            className="flex h-40 items-center justify-center bg-orange-50 text-5xl font-bold text-orange-300"
          >
            {restaurant.name.charAt(0)}
          </div>
        )}
        <div className="flex flex-col gap-4 p-6 sm:flex-row sm:items-start sm:justify-between">
          <div className="min-w-0 flex-1 space-y-2">
            <h1 className="text-3xl font-bold">{restaurant.name}</h1>
            <p className="text-gray-600">
              {cuisineLabel(restaurant.cuisine)} · {restaurant.suburb} ·{' '}
              <span aria-label={`Price level ${restaurant.priceLevel} of 4`}>{'$'.repeat(restaurant.priceLevel)}</span>
            </p>
            <p className="text-sm text-gray-600">{restaurant.address}</p>
            {restaurant.description && <p className="max-w-prose text-gray-800">{restaurant.description}</p>}
          </div>
          <BookButton />
        </div>
      </div>

      <section className="space-y-4">
        <h2 className="text-2xl font-semibold">Menu</h2>
        {menu.length === 0 || menu.every((category) => category.items.length === 0) ? (
          <p className="text-gray-600">This restaurant has not added its menu yet.</p>
        ) : (
          menu
            .filter((category) => category.items.length > 0)
            .map((category) => <MenuSection key={category.id} category={category} />)
        )}
      </section>
    </article>
  )
}

// Guests are sent to sign up and brought back here afterwards; for signed in users the button does nothing until the booking flow is built
function BookButton() {
  const { isAuthenticated } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()

  function handleClick() {
    if (!isAuthenticated) {
      const state: RedirectState = { from: location.pathname }
      navigate(ROUTES.register, { state })
    }
  }

  return (
    <Button className="shrink-0 self-start" onClick={handleClick}>
      Book a table
    </Button>
  )
}

function MenuSection({ category }: { category: MenuCategory }) {
  return (
    <section className="rounded border border-gray-200 bg-white p-6">
      <h3 className="mb-2 text-lg font-semibold">{category.name}</h3>
      <ul className="divide-y divide-gray-100">
        {category.items.map((item) => (
          <MenuItemRow key={item.id} item={item} />
        ))}
      </ul>
    </section>
  )
}

function MenuItemRow({ item }: { item: MenuItem }) {
  return (
    <li className="flex items-center gap-4 py-3">
      {item.imageUrl && (
        <img src={apiUrl(item.imageUrl)} alt={item.name} className="h-16 w-16 shrink-0 rounded object-cover" />
      )}
      <div className="min-w-0 flex-1">
        <p className="font-medium">{item.name}</p>
        {item.description && <p className="text-sm text-gray-600">{item.description}</p>}
      </div>
      <p className="shrink-0 font-medium">{formatPrice(item.price)}</p>
    </li>
  )
}
