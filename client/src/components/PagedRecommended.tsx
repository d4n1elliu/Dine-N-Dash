import { useCallback } from 'react'
import { useSearchParams } from 'react-router-dom'
import { getRecommendedRestaurants } from '../api/restaurants'
import { useApi } from '../hooks/useApi'
import { readPageParam } from '../lib/page-param'
import RestaurantResults from './RestaurantResults'

// The full recommended list for signed in customers, with the page number kept in the URL
export default function PagedRecommended() {
  const [searchParams, setSearchParams] = useSearchParams()
  const page = readPageParam(searchParams)

  const fetchRecommended = useCallback(() => getRecommendedRestaurants(page), [page])
  const recommended = useApi(fetchRecommended)

  function handlePageChange(nextPage: number) {
    setSearchParams(nextPage > 1 ? { page: String(nextPage) } : {})
    window.scrollTo({ top: 0 })
  }

  return (
    <div className="space-y-4">
      <h2 className="text-xl font-semibold">Recommended restaurants</h2>
      <RestaurantResults
        results={recommended}
        emptyMessage="No restaurants yet. Check back soon."
        onPageChange={handlePageChange}
      />
    </div>
  )
}
