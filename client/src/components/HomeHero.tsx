import { Link } from 'react-router-dom'
import { getRecommendedRestaurants, type Restaurant } from '../api/restaurants'
import { useApi } from '../hooks/useApi'
import { restaurantPath } from '../routes'
import RestaurantImage from './RestaurantImage'
import SearchBar from './SearchBar'

const PHOTO_COUNT = 4

// Always the top restaurants, whichever page of the list below is showing
const fetchTopRestaurants = () => getRecommendedRestaurants(1, PHOTO_COUNT)

export default function HomeHero() {
  const top = useApi(fetchTopRestaurants)
  const restaurants = top.data?.items ?? []
  const showPhotos = restaurants.length === PHOTO_COUNT
  // Keep the second column while loading so the heading doesn't rewrap when the photos arrive
  const twoColumns = top.loading || showPhotos

  return (
    <div className={`grid items-center gap-8 lg:py-6 ${twoColumns ? 'lg:grid-cols-2' : ''}`}>
      <div className="space-y-6">
        <h1 className="text-4xl font-bold tracking-tight sm:text-5xl lg:text-6xl">Find your next table.</h1>
        <SearchBar />
      </div>
      {showPhotos && <PhotoGrid restaurants={restaurants} />}
    </div>
  )
}

function PhotoGrid({ restaurants }: { restaurants: Restaurant[] }) {
  return (
    <ul className="grid grid-cols-2 gap-3">
      {restaurants.map((restaurant) => (
        <li key={restaurant.id}>
          {/* The label names the link, since a restaurant with no photo shows only a letter */}
          <Link
            to={restaurantPath(restaurant.id)}
            aria-label={restaurant.name}
            className="block overflow-hidden rounded-xl transition hover:shadow-md focus-visible:ring-2 focus-visible:ring-orange-500 focus-visible:ring-offset-2 focus-visible:outline-none"
          >
            <RestaurantImage
              imageUrl={restaurant.profileImageUrl}
              name={restaurant.name}
              alt=""
              className="aspect-[5/4] text-4xl"
            />
          </Link>
        </li>
      ))}
    </ul>
  )
}
