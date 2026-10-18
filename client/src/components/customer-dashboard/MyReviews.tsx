import type { ChangeEvent } from 'react'
import { useState } from 'react'
import { deleteReview, getMyReviews, updateReview, type Review } from '../../api/reviews'
import { useApi } from '../../hooks/useApi'
import { useSubmit } from '../../hooks/useSubmit'
import { dangerButtonClass, smallPrimaryButtonClass } from '../../lib/form-styles'
import { restaurantPath, ROUTES } from '../../routes'
import FormError from '../FormError'
import TextLink from '../TextLink'
import { MAX_COMMENT_LENGTH } from '../../lib/reviews'
import Stars from '../reviews/Stars'

const dateFormat = new Intl.DateTimeFormat('en-AU', { dateStyle: 'medium' })
const secondaryButtonClass = 'rounded border border-gray-300 px-3 py-1 text-sm hover:bg-gray-50'
const inputClass =
  'w-full rounded border border-gray-300 bg-white px-3 py-2 text-sm focus:border-orange-500 focus:outline-none'

export default function MyReviews() {
  const reviews = useApi(getMyReviews)

  if (reviews.loading && !reviews.data) return <p className="text-gray-500">Loading your reviews...</p>
  if (reviews.error && !reviews.data) return <p className="text-red-700">{reviews.error}</p>

  const list = reviews.data ?? []

  if (list.length === 0) {
    return (
      <p className="text-gray-600">
        You have not written any reviews yet. <TextLink to={ROUTES.search}>Find a restaurant</TextLink> you have
        visited and share what you thought.
      </p>
    )
  }

  return (
    <ul className="space-y-3">
      {list.map((review) => (
        <ReviewCard key={review.id} review={review} onChanged={reviews.reload} />
      ))}
    </ul>
  )
}

function ReviewCard({ review, onChanged }: { review: Review; onChanged: () => void }) {
  const [editing, setEditing] = useState(false)
  const [confirmingDelete, setConfirmingDelete] = useState(false)
  const [rating, setRating] = useState(String(review.rating))
  const [comment, setComment] = useState(review.comment)
  const { submitting, error, setError, run } = useSubmit('Something went wrong. Please try again.')

  const onFieldChange =
    (setValue: (value: string) => void) => (event: ChangeEvent<HTMLSelectElement | HTMLTextAreaElement>) => {
      setValue(event.target.value)
      setError(null)
    }

  function save() {
    if (comment.trim().length === 0) return setError('Write a comment.')
    if (comment.length > MAX_COMMENT_LENGTH) return setError(`Keep your comment under ${MAX_COMMENT_LENGTH} characters.`)

    void run(async () => {
      await updateReview(review.id, { rating: Number(rating), comment: comment.trim() })
      setEditing(false)
      onChanged()
    })
  }

  function remove() {
    void run(async () => {
      await deleteReview(review.id)
      onChanged()
    })
  }

  function startEditing() {
    setRating(String(review.rating))
    setComment(review.comment)
    setError(null)
    setEditing(true)
  }

  return (
    <li className="space-y-3 rounded border border-gray-200 bg-white p-4">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <p className="font-medium">
          <TextLink to={restaurantPath(review.restaurantId)}>{review.restaurantName ?? 'Restaurant'}</TextLink>
        </p>
        <p className="text-sm text-gray-600">{dateFormat.format(new Date(review.createdAt))}</p>
      </div>

      {editing ? (
        <div className="space-y-3">
          <label className="block space-y-1 text-sm font-medium">
            Rating
            <select value={rating} onChange={onFieldChange(setRating)} className={inputClass}>
              {[5, 4, 3, 2, 1].map((value) => (
                <option key={value} value={value}>
                  {value} {value === 1 ? 'star' : 'stars'}
                </option>
              ))}
            </select>
          </label>
          <label className="block space-y-1 text-sm font-medium">
            Comment
            <textarea rows={4} value={comment} onChange={onFieldChange(setComment)} className={inputClass} />
            <span className="text-xs font-normal text-gray-500">
              {comment.length}/{MAX_COMMENT_LENGTH}
            </span>
          </label>
          <div className="flex gap-2">
            <button type="button" className={smallPrimaryButtonClass} disabled={submitting} onClick={save}>
              {submitting ? 'Saving...' : 'Save'}
            </button>
            <button type="button" className={secondaryButtonClass} disabled={submitting} onClick={() => setEditing(false)}>
              Cancel
            </button>
          </div>
        </div>
      ) : (
        <>
          <Stars rating={review.rating} />
          <p className="text-gray-800">{review.comment}</p>
          <div className="flex gap-2">
            {confirmingDelete ? (
              <>
                <button type="button" className={dangerButtonClass} disabled={submitting} onClick={remove}>
                  {submitting ? 'Deleting...' : 'Confirm delete'}
                </button>
                <button type="button" className={secondaryButtonClass} disabled={submitting} onClick={() => setConfirmingDelete(false)}>
                  Never mind
                </button>
              </>
            ) : (
              <>
                <button type="button" className={secondaryButtonClass} onClick={startEditing}>
                  Edit
                </button>
                <button type="button" className={secondaryButtonClass} onClick={() => setConfirmingDelete(true)}>
                  Delete
                </button>
              </>
            )}
          </div>
        </>
      )}
      <FormError message={error} />
    </li>
  )
}