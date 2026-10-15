import { useEffect, useState } from 'react'
import { getErrorMessage } from '../api/client'

export interface ApiState<T> {
  data: T | null
  error: string | null
  loading: boolean
}

// Use a stable fetcher to avoid refetching on every render
export function useApi<T>(fetcher: () => Promise<T>): ApiState<T> {
  const [state, setState] = useState<ApiState<T>>({ data: null, error: null, loading: true })

  useEffect(() => {
    let cancelled = false

    fetcher()
      .then((data) => {
        if (!cancelled) setState({ data, error: null, loading: false })
      })
      .catch((error: unknown) => {
        if (!cancelled) setState({ data: null, error: getErrorMessage(error), loading: false })
      })

    return () => {
      cancelled = true
    }
  }, [fetcher])

  return state
}
