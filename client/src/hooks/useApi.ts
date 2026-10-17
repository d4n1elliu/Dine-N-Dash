import { useCallback, useEffect, useState } from 'react'
import { getErrorMessage } from '../api/client'

export interface ApiState<T> {
  data: T | null
  error: string | null
  loading: boolean
}

export interface ApiResult<T> extends ApiState<T> {
  // Fetches again, keeping the current data on screen until the new response arrives
  reload: () => void
}

// Use a stable fetcher to avoid refetching on every render
export function useApi<T>(fetcher: () => Promise<T>): ApiResult<T> {
  const [state, setState] = useState<ApiState<T>>({ data: null, error: null, loading: true })
  const [version, setVersion] = useState(0)
  const reload = useCallback(() => setVersion((current) => current + 1), [])

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
  }, [fetcher, version])

  return { ...state, reload }
}
