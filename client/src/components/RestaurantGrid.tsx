import type { Restaurant } from '../api/restaurants'
import RestaurantCard from './RestaurantCard'

// One column on phones, two on tablets and three on desktops
export default function RestaurantGrid({ restaurants }: { restaurants: Restaurant[] }) {
  return (
    <ul className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
      {restaurants.map((restaurant) => (
        <li key={restaurant.id}>
          <RestaurantCard restaurant={restaurant} />
        </li>
      ))}
    </ul>
  )
}
