import type { ChangeEvent, FormEvent } from 'react'
import { useState } from 'react'
import { createReview } from '../../api/reviews'
import { useSubmit } from '../../hooks/useSubmit'
import { MAX_COMMENT_LENGTH } from '../../lib/reviews'
import Button from '../Button'
import FormError from '../FormError'

const inputClass =
  'w-full rounded border border-gray-300 bg-white px-3 py-2 text-sm focus:border-orange-500 focus:outline-none'

interface ReviewFormProps {
  restaurantId: number
  onCreated: () => void
}

export default function ReviewForm({ restaurantId, onCreated }: ReviewFormProps) {
  const [rating, setRating] = useState(0) // 0 means nothing chosen yet
  const [comment, setComment] = useState('')
  const { submitting, error, setError, run } = useSubmit('Could not post your review. Please try again.')

  function chooseRating(value: number) {
    setRating(value)
    setError(null)
  }

  function handleCommentChange(event: ChangeEvent<HTMLTextAreaElement>) {
    setComment(event.target.value)
    setError(null)
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    if (rating === 0) return setError('Choose a rating.')
    if (comment.trim().length === 0) return setError('Write a comment.')
    if (comment.length > MAX_COMMENT_LENGTH) {
      return setError(`Keep your comment under ${MAX_COMMENT_LENGTH} characters.`)
    }

    void run(async () => {
      await createReview(restaurantId, { rating, comment: comment.trim() })
      setRating(0)
      setComment('')
      onCreated()
    })
  }

  return (
    <form onSubmit={handleSubmit} noValidate className="space-y-4 rounded border border-gray-200 bg-white p-6">
      <h3 className="text-lg font-semibold">Write a review</h3>

      <div>
        <p className="mb-1 text-sm font-medium">Your rating</p>
        <div role="radiogroup" aria-label="Rating" className="flex gap-1">
          {[1, 2, 3, 4, 5].map((value) => (
            <button
              key={value}
              type="button"
              role="radio"
              aria-checked={rating === value}
              aria-label={`${value} ${value === 1 ? 'star' : 'stars'}`}
              onClick={() => chooseRating(value)}
              className={`text-3xl leading-none ${value <= rating ? 'text-amber-500' : 'text-gray-300'}`}
            >
              ★
            </button>
          ))}
        </div>
      </div>

      <label className="block space-y-1 text-sm font-medium">
        Comment
        <textarea
          rows={4}
          value={comment}
          onChange={handleCommentChange}
          className={inputClass}
          placeholder="What did you think of the food and service?"
        />
        <span className="text-xs font-normal text-gray-500">
          {comment.length}/{MAX_COMMENT_LENGTH}
        </span>
      </label>

      <FormError message={error} />
      <Button type="submit" disabled={submitting}>
        {submitting ? 'Posting...' : 'Post review'}
      </Button>
    </form>
  )
}