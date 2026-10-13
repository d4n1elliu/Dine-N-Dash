import { Link } from 'react-router-dom'
import { ROUTES } from '../routes'

export default function NotFoundPage() {
  return (
    <section className="space-y-4 text-center">
      <h1 className="text-3xl font-bold">Page not found</h1>
      <p className="text-gray-600">That page does not exist.</p>
      <Link to={ROUTES.home} className="text-orange-600 underline">
        Back to home
      </Link>
    </section>
  )
}
