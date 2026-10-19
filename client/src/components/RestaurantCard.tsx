import { Link } from 'react-router-dom'
import { cuisineLabel, type Restaurant } from '../api/restaurants'
import { restaurantPath } from '../routes'
import RestaurantImage from './RestaurantImage'

export default function RestaurantCard({ restaurant }: { restaurant: Restaurant }) {
  return (
    <Link
      to={restaurantPath(restaurant.id)}
      className="flex h-full flex-col overflow-hidden rounded border border-gray-200 bg-white transition hover:border-orange-300 hover:shadow-md focus-visible:ring-2 focus-visible:ring-orange-500 focus-visible:outline-none"
    >
      {/* The heading names the link, so the photo stays decorative */}
      <RestaurantImage imageUrl={restaurant.profileImageUrl} name={restaurant.name} alt="" className="h-40 text-3xl" />
      <div className="flex flex-1 flex-col gap-1 p-4">
        <h3 className="font-semibold">{restaurant.name}</h3>
        <p className="text-sm text-gray-600">
          {cuisineLabel(restaurant.cuisine)} · {restaurant.suburb} ·{' '}
          <span aria-label={`Price level ${restaurant.priceLevel} of 4`}>{'$'.repeat(restaurant.priceLevel)}</span>
        </p>
        {restaurant.description && (
          <p className="line-clamp-2 text-sm text-gray-700">{restaurant.description}</p>
        )}
      </div>
    </Link>
  )
}
