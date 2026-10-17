import { apiUrl } from '../api/client'
import { cuisineLabel, type Restaurant } from '../api/restaurants'

export default function RestaurantCard({ restaurant }: { restaurant: Restaurant }) {
  const photo = restaurant.profileImageUrl ? apiUrl(restaurant.profileImageUrl) : null

  return (
    <article className="flex flex-col overflow-hidden rounded border border-gray-200 bg-white">
      {photo ? (
        <img src={photo} alt={restaurant.name} className="h-40 w-full object-cover" />
      ) : (
        <div className="flex h-40 items-center justify-center bg-orange-50 text-3xl font-bold text-orange-300">
          {restaurant.name.charAt(0)}
        </div>
      )}
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
    </article>
  )
}
