import { Link } from 'react-router-dom'
import type { Restaurant } from '../api/restaurants'
import { useApi } from '../hooks/useApi'
import { HERO_PHOTO_COUNT } from '../lib/home-hero'
import { restaurantPath } from '../routes'
import RestaurantImage from './RestaurantImage'
import SearchBar from './SearchBar'

interface HomeHeroProps {
  // Must be a stable function. Only the first few restaurants it returns are shown
  fetchPhotos: () => Promise<Restaurant[]>
}

export default function HomeHero({ fetchPhotos }: HomeHeroProps) {
  const photos = useApi(fetchPhotos)
  const restaurants = (photos.data ?? []).slice(0, HERO_PHOTO_COUNT)
  const showPhotos = restaurants.length === HERO_PHOTO_COUNT
  // Keep the second column while loading so the heading doesn't rewrap when the photos arrive
  const twoColumns = photos.loading || showPhotos

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
