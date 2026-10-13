import { useAuth } from '../context/useAuth'

export default function DashboardPage() {
  const { user } = useAuth()

  return (
    <section className="space-y-2">
      <h1 className="text-3xl font-bold">Dashboard</h1>
      <p className="text-gray-600">
        Welcome back{user ? `, ${user.name}` : ''}. Your bookings and reviews will appear here.
      </p>
    </section>
  )
}
