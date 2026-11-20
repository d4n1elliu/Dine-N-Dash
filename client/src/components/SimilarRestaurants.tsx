import { useCallback } from 'react'
import { getSimilarRestaurants } from '../api/restaurants'
import { useApi } from '../hooks/useApi'
import RestaurantGrid from './RestaurantGrid'

export default function SimilarRestaurants({ restaurantId }: { restaurantId: number }) {
  const fetchSimilar = useCallback(() => getSimilarRestaurants(restaurantId), [restaurantId])
  const result = useApi(fetchSimilar)

  // Hide the whole section, heading included, when nothing is similar
  if (result.data?.length === 0) return null

  return (
    <section className="space-y-4">
      <h2 className="text-2xl font-semibold">Similar restaurants</h2>
      {result.loading && <p className="text-gray-500">Loading similar restaurants...</p>}
      {result.error && <p className="text-red-700">{result.error}</p>}
      {result.data && <RestaurantGrid restaurants={result.data} />}
    </section>
  )
}
