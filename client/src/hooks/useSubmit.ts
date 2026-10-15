import { useCallback, useState } from 'react'
import { getErrorMessage } from '../api/client'

export interface SubmitState {
  submitting: boolean
  error: string | null
  setError: (error: string | null) => void
  run: (action: () => Promise<void>) => Promise<void>
}

export function useSubmit(fallbackMessage: string): SubmitState {
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const run = useCallback(
    async (action: () => Promise<void>) => {
      setError(null)
      setSubmitting(true)
      try {
        await action()
      } catch (err) {
        setError(getErrorMessage(err, fallbackMessage))
      } finally {
        setSubmitting(false)
      }
    },
    [fallbackMessage],
  )

  return { submitting, error, setError, run }
}
