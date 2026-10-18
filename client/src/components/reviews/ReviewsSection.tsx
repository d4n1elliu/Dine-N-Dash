import { useCallback } from 'react'
import { useLocation } from 'react-router-dom'
import { getAverageRating, getMyReviews, getRestaurantReviews } from '../../api/reviews'
import { useAuth } from '../../context/useAuth'
import { useApi } from '../../hooks/useApi'
import { ROUTES, type RedirectState } from '../../routes'
import TextLink from '../TextLink'
import ReviewForm from './ReviewForm'
import Stars from './Stars'

const dateFormat = new Intl.DateTimeFormat('en-AU', { dateStyle: 'medium' })

// Loads the reviews and the average together so there's no discrepancy 
async function loadReviews(restaurantId: number) {
  const [reviews, average] = await Promise.all([getRestaurantReviews(restaurantId), getAverageRating(restaurantId)])
  return { reviews, average: average.average }
}

export default function ReviewsSection({ restaurantId }: { restaurantId: number }) {
  const fetchReviews = useCallback(() => loadReviews(restaurantId), [restaurantId])
  const result = useApi(fetchReviews)

  const reviews = result.data?.reviews ?? []
  const average = result.data?.average ?? 0

  return (
    <section className="space-y-4">
      <h2 className="text-2xl font-semibold">Reviews</h2>

      {result.loading && !result.data && <p className="text-gray-500">Loading reviews...</p>}
      {result.error && !result.data && <p className="text-red-700">{result.error}</p>}

      {result.data && (
        <>
          {reviews.length === 0 ? (
            <p className="text-gray-600">No reviews yet. Be the first to share what you thought.</p>
          ) : (
            <>
              <p className="flex items-center gap-2">
                <span className="text-3xl font-bold">{average.toFixed(1)}</span>
                <Stars rating={Math.round(average)} />
                <span className="text-sm text-gray-600">
                  ({reviews.length} {reviews.length === 1 ? 'review' : 'reviews'})
                </span>
              </p>
              <ul className="space-y-3">
                {reviews.map((review) => (
                  <li key={review.id} className="space-y-1 rounded border border-gray-200 bg-white p-4">
                    <Stars rating={review.rating} />
                    <p className="text-gray-800">{review.comment}</p>
                    <p className="text-sm text-gray-600">
                      {review.dinerName ?? 'A diner'} · {dateFormat.format(new Date(review.createdAt))}
                    </p>
                  </li>
                ))}
              </ul>
            </>
          )}
        </>
      )}

      <WriteReview restaurantId={restaurantId} onReviewAdded={result.reload} />
    </section>
  )
}

// Guests are asked to log in, owners see nothing, and diners get the form unless they already reviewed this restaurant
function WriteReview({ restaurantId, onReviewAdded }: { restaurantId: number; onReviewAdded: () => void }) {
  const { isAuthenticated, user } = useAuth()
  const location = useLocation()

  if (!isAuthenticated) {
    const state: RedirectState = { from: location.pathname }
    return (
      <p className="text-gray-600">
        <TextLink to={ROUTES.login} state={state}>
          Log in
        </TextLink>{' '}
        to write a review.
      </p>
    )
  }

  if (user?.role !== 'Diner') return null

  return <DinerReviewArea restaurantId={restaurantId} onReviewAdded={onReviewAdded} />
}

function DinerReviewArea({ restaurantId, onReviewAdded }: { restaurantId: number; onReviewAdded: () => void }) {
  const myReviews = useApi(getMyReviews)

  if (myReviews.loading && !myReviews.data) return null

  const alreadyReviewed = myReviews.data?.some((review) => review.restaurantId === restaurantId)

  if (alreadyReviewed) {
    return (
      <p className="text-gray-600">
        You have reviewed this restaurant. You can edit or delete your review from your{' '}
        <TextLink to={`${ROUTES.dashboard}?tab=reviews`}>dashboard</TextLink>.
      </p>
    )
  }

  return (
    <ReviewForm
      restaurantId={restaurantId}
      onCreated={() => {
        myReviews.reload()
        onReviewAdded()
      }}
    />
  )
}