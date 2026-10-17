interface PaginationProps {
  page: number
  totalPages: number
  onPageChange: (page: number) => void
}

const pageButtonClass =
  'rounded border border-gray-300 bg-white px-3 py-1 text-sm font-medium hover:bg-orange-100 disabled:opacity-50 disabled:hover:bg-white'

export default function Pagination({ page, totalPages, onPageChange }: PaginationProps) {
  if (totalPages <= 1) return null

  return (
    <nav aria-label="Pagination" className="flex items-center justify-center gap-3">
      <button
        type="button"
        className={pageButtonClass}
        disabled={page <= 1}
        onClick={() => onPageChange(page - 1)}
      >
        Previous
      </button>
      <span className="text-sm text-gray-600">
        Page {page} of {totalPages}
      </span>
      <button
        type="button"
        className={pageButtonClass}
        disabled={page >= totalPages}
        onClick={() => onPageChange(page + 1)}
      >
        Next
      </button>
    </nav>
  )
}
