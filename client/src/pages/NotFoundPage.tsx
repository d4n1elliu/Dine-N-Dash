import TextLink from '../components/TextLink'
import { ROUTES } from '../routes'

export default function NotFoundPage() {
  return (
    <section className="space-y-4 text-center">
      <h1 className="text-3xl font-bold">Page not found</h1>
      <p className="text-gray-600">That page does not exist.</p>
      <TextLink to={ROUTES.home}>
        Back to home
      </TextLink>
    </section>
  )
}
