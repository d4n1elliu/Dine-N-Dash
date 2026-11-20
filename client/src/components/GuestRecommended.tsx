import { useCallback } from 'react'
import { Link } from 'react-router-dom'
import { getRecommendedRestaurants } from '../api/restaurants'
import { useApi } from '../hooks/useApi'
import { buttonClass } from '../lib/button-styles'
import { ROUTES } from '../routes'
import RestaurantResults from './RestaurantResults'

// A short, fixed list for guests, with a link to browse everything
export default function GuestRecommended({ count }: { count: number }) {
  const fetchRecommended = useCallback(() => getRecommendedRestaurants(1, count), [count])
  const recommended = useApi(fetchRecommended)

  return (
    <div className="space-y-6">
      <h2 className="text-xl font-semibold">Recommended restaurants</h2>
      <RestaurantResults results={recommended} emptyMessage="No restaurants yet. Check back soon." />
      <p className="text-center">
        <Link to={ROUTES.search} className={`inline-block ${buttonClass()}`}>
          View all restaurants
        </Link>
      </p>
    </div>
  )
}
