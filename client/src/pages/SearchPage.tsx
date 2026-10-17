import { useCallback } from 'react'
import { useSearchParams } from 'react-router-dom'
import { CUISINES, cuisineLabel, searchRestaurants, type Cuisine, type RestaurantSort } from '../api/restaurants'
import RestaurantResults from '../components/RestaurantResults'
import SearchBar from '../components/SearchBar'
import { useApi } from '../hooks/useApi'
import { readPageParam } from '../lib/page-param'

const SORT_OPTIONS: { value: RestaurantSort; label: string }[] = [
  { value: 'name', label: 'Name' },
  { value: 'rating', label: 'Rating' },
  { value: 'price', label: 'Price' },
]

const selectClass = 'rounded border border-gray-300 bg-white px-3 py-2 text-sm focus:border-orange-500 focus:outline-none'

function readCuisine(value: string | null): Cuisine | undefined {
  return CUISINES.find((cuisine) => cuisine === value)
}

function readSort(value: string | null): RestaurantSort {
  return SORT_OPTIONS.find((option) => option.value === value)?.value ?? 'name'
}

// Keeps the search in the URL so results can be shared, refreshed and navigated with the back button
export default function SearchPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const name = searchParams.get('name') ?? ''
  const cuisine = readCuisine(searchParams.get('cuisine'))
  const sortBy = readSort(searchParams.get('sortBy'))
  const page = readPageParam(searchParams)

  const fetchResults = useCallback(
    () => searchRestaurants({ name, cuisine, sortBy, page, pageSize: 9 }),
    [name, cuisine, sortBy, page],
  )
  const results = useApi(fetchResults)

  // Any change other than the page starts again from page 1
  function updateParam(key: string, value: string) {
    const next = new URLSearchParams(searchParams)
    if (value) next.set(key, value)
    else next.delete(key)
    if (key !== 'page') next.delete('page')
    setSearchParams(next)
  }

  function handlePageChange(nextPage: number) {
    updateParam('page', nextPage > 1 ? String(nextPage) : '')
    window.scrollTo({ top: 0 })
  }

  const total = results.data?.totalCount

  return (
    <section className="space-y-6">
      <h1 className="text-3xl font-bold">{name ? `Results for "${name}"` : 'All restaurants'}</h1>

      <div className="space-y-3">
        <SearchBar key={name} initialValue={name} onSearch={(query) => updateParam('name', query)} />
        <div className="flex flex-wrap gap-3">
          <label className="flex items-center gap-2 text-sm font-medium">
            Cuisine
            <select
              value={cuisine ?? ''}
              onChange={(e) => updateParam('cuisine', e.target.value)}
              className={selectClass}
            >
              <option value="">Any</option>
              {CUISINES.map((option) => (
                <option key={option} value={option}>
                  {cuisineLabel(option)}
                </option>
              ))}
            </select>
          </label>
          <label className="flex items-center gap-2 text-sm font-medium">
            Sort by
            <select
              value={sortBy}
              onChange={(e) => updateParam('sortBy', e.target.value === 'name' ? '' : e.target.value)}
              className={selectClass}
            >
              {SORT_OPTIONS.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </label>
        </div>
      </div>

      {total !== undefined && total > 0 && (
        <p className="text-sm text-gray-600">
          {total} {total === 1 ? 'restaurant' : 'restaurants'} found
        </p>
      )}

      <RestaurantResults
        results={results}
        emptyMessage="No restaurants match your search."
        onPageChange={handlePageChange}
      />
    </section>
  )
}
