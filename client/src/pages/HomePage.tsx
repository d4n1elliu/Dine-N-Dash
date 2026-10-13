import { getHealth } from '../api/health'
import { useApi } from '../hooks/useApi'

export default function HomePage() {
  const health = useApi(getHealth)

  return (
    <section className="space-y-4">
      <h1 className="text-3xl font-bold">Welcome to Dine N Dash</h1>
      <p className="text-gray-600">Search restaurants, book a table and leave reviews.</p>

      <div className="rounded border border-gray-200 bg-white p-4">
        <h2 className="mb-2 font-semibold">API status</h2>
        {health.loading && <p className="text-gray-500">Checking...</p>}
        {health.data && <p className="text-green-700">{health.data.status}</p>}
        {health.error && <p className="text-red-700">{health.error}</p>}
      </div>
    </section>
  )
}
