export default function Stars({ rating }: { rating: number }) {
  return (
    <span aria-label={`${rating} out of 5 stars`} className="text-amber-500">
      {'★'.repeat(rating)}
      {'☆'.repeat(5 - rating)}
    </span>
  )
}