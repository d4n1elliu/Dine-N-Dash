import type { Restaurant } from '../api/restaurants'
import RestaurantCard from './RestaurantCard'

// One column on phones, two on tablets and three on desktops
const gridClass = 'grid gap-4 sm:grid-cols-2 lg:grid-cols-3'

export default function RestaurantGrid({ restaurants }: { restaurants: Restaurant[] }) {
  return (
    <ul className={gridClass}>
      {restaurants.map((restaurant) => (
        <li key={restaurant.id}>
          <RestaurantCard restaurant={restaurant} />
        </li>
      ))}
    </ul>
  )
}

// Grey placeholder cards in the same layout, shown while the restaurants load
export function RestaurantGridSkeleton({ count }: { count: number }) {
  return (
    <div role="status">
      <span className="sr-only">Loading restaurants...</span>
      <ul aria-hidden="true" className={`${gridClass} animate-pulse`}>
        {Array.from({ length: count }, (_, index) => (
          <li key={index} className="overflow-hidden rounded border border-gray-200 bg-white">
            <div className="h-40 bg-gray-200" />
            <div className="space-y-2 p-4">
              <div className="h-4 w-2/3 rounded bg-gray-200" />
              <div className="h-3 w-1/2 rounded bg-gray-200" />
              <div className="h-3 w-full rounded bg-gray-200" />
            </div>
          </li>
        ))}
      </ul>
    </div>
  )
}
