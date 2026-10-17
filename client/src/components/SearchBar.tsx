import type { FormEvent } from 'react'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { ROUTES } from '../routes'
import Button from './Button'

interface SearchBarProps {
  initialValue?: string
  // Defaults to opening the search results page with the name in the URL
  onSearch?: (name: string) => void
}

export default function SearchBar({ initialValue = '', onSearch }: SearchBarProps) {
  const [name, setName] = useState(initialValue)
  const navigate = useNavigate()

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const query = name.trim()
    if (onSearch) {
      onSearch(query)
      return
    }
    navigate(query ? `${ROUTES.search}?${new URLSearchParams({ name: query })}` : ROUTES.search)
  }

  return (
    <form role="search" onSubmit={handleSubmit} className="flex gap-2">
      <label htmlFor="restaurant-search" className="sr-only">
        Search restaurants by name
      </label>
      <input
        id="restaurant-search"
        type="search"
        placeholder="Search restaurants by name"
        value={name}
        onChange={(e) => setName(e.target.value)}
        className="w-full rounded border border-gray-300 px-3 py-2 focus:border-orange-500 focus:outline-none"
      />
      <Button type="submit">Search</Button>
    </form>
  )
}
