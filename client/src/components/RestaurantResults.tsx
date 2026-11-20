import type { PagedResponse, Restaurant } from '../api/restaurants'
import type { ApiState } from '../hooks/useApi'
import Pagination from './Pagination'
import RestaurantGrid from './RestaurantGrid'

interface RestaurantResultsProps {
  results: ApiState<PagedResponse<Restaurant>>
  emptyMessage: string
  // Leave out to show the results without page controls
  onPageChange?: (page: number) => void
}

// Shared by the home and search pages: loading, error and empty states, the card grid and page controls
export default function RestaurantResults({ results, emptyMessage, onPageChange }: RestaurantResultsProps) {
  if (results.loading) return <p className="text-gray-500">Loading restaurants...</p>
  if (results.error) return <p className="text-red-700">{results.error}</p>
  if (!results.data || results.data.items.length === 0) return <p className="text-gray-600">{emptyMessage}</p>

  const { items, page, totalPages } = results.data

  return (
    <div className="space-y-6">
      <RestaurantGrid restaurants={items} />
      {onPageChange && <Pagination page={page} totalPages={totalPages} onPageChange={onPageChange} />}
    </div>
  )
}
