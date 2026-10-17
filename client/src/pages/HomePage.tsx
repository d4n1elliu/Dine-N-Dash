import { useCallback } from 'react'
import { useSearchParams } from 'react-router-dom'
import { getRecommendedRestaurants } from '../api/restaurants'
import RestaurantResults from '../components/RestaurantResults'
import SearchBar from '../components/SearchBar'
import { useApi } from '../hooks/useApi'
import { readPageParam } from '../lib/page-param'

export default function HomePage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const page = readPageParam(searchParams)

  const fetchRecommended = useCallback(() => getRecommendedRestaurants(page), [page])
  const recommended = useApi(fetchRecommended)

  function handlePageChange(nextPage: number) {
    setSearchParams(nextPage > 1 ? { page: String(nextPage) } : {})
    window.scrollTo({ top: 0 })
  }

  return (
    <section className="space-y-8">
      <div className="space-y-4">
        <h1 className="text-3xl font-bold">Welcome to Dine N Dash</h1>
        <p className="text-gray-600">Search restaurants, book a table and leave reviews.</p>
        <SearchBar />
      </div>

      <div className="space-y-4">
        <h2 className="text-xl font-semibold">Recommended restaurants</h2>
        <RestaurantResults
          results={recommended}
          emptyMessage="No restaurants yet. Check back soon."
          onPageChange={handlePageChange}
        />
      </div>
    </section>
  )
}
