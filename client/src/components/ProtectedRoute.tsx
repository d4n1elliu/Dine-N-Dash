import { Navigate, Outlet, useLocation } from 'react-router-dom'
import type { UserRole } from '../api/auth'
import { useAuth } from '../context/useAuth'
import { dashboardFor, ROUTES, type RedirectState } from '../routes'

// With a role, users of the other role are sent to their own dashboard instead
export default function ProtectedRoute({ role }: { role?: UserRole }) {
  const { isAuthenticated, user } = useAuth()
  const location = useLocation()

  if (!isAuthenticated) {
    const state: RedirectState = { from: location.pathname }
    return <Navigate to={ROUTES.login} replace state={state} />
  }

  if (role && user?.role !== role) {
    return <Navigate to={dashboardFor(user?.role)} replace />
  }

  return <Outlet />
}
