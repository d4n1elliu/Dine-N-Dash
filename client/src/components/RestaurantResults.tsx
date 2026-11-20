import type { PagedResponse, Restaurant } from '../api/restaurants'
import type { ApiState } from '../hooks/useApi'
import { outlineButtonClass } from '../lib/form-styles'
import Pagination from './Pagination'
import RestaurantGrid, { RestaurantGridSkeleton } from './RestaurantGrid'

interface RestaurantResultsProps {
  results: ApiState<PagedResponse<Restaurant>>
  emptyMessage: string
  // How many placeholder cards to show while loading
  skeletonCount?: number
  // Leave out to show the results without page controls
  onPageChange?: (page: number) => void
  // Shows a "Try again" button when the request fails
  onRetry?: () => void
}

// Shared by the home and search pages: loading, error and empty states, the card grid and page controls
export default function RestaurantResults({
  results,
  emptyMessage,
  skeletonCount = 9,
  onPageChange,
  onRetry,
}: RestaurantResultsProps) {
  if (results.loading) return <RestaurantGridSkeleton count={skeletonCount} />

  if (results.error) {
    return (
      <div role="alert" className="space-y-3 rounded border border-red-200 bg-red-50 p-6 text-center">
        <p className="text-red-700">{results.error}</p>
        {onRetry && (
          <button type="button" className={outlineButtonClass} onClick={onRetry}>
            Try again
          </button>
        )}
      </div>
    )
  }

  if (!results.data || results.data.items.length === 0) {
    return (
      <p className="rounded border border-dashed border-gray-300 bg-white p-8 text-center text-gray-600">
        {emptyMessage}
      </p>
    )
  }

  const { items, page, totalPages } = results.data

  return (
    <div className="space-y-6">
      <RestaurantGrid restaurants={items} />
      {onPageChange && <Pagination page={page} totalPages={totalPages} onPageChange={onPageChange} />}
    </div>
  )
}
