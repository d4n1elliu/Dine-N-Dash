import { useCallback } from 'react'
import { useSearchParams } from 'react-router-dom'
import { getRecommendedRestaurants } from '../api/restaurants'
import HomeHero from '../components/HomeHero'
import RestaurantResults from '../components/RestaurantResults'
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
      <HomeHero />

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
