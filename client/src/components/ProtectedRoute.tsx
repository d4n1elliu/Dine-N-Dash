import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { useAuth } from '../context/useAuth'
import { ROUTES, type RedirectState } from '../routes'

export default function ProtectedRoute() {
  const { isAuthenticated } = useAuth()
  const location = useLocation()

  if (!isAuthenticated) {
    const state: RedirectState = { from: location.pathname }
    return <Navigate to={ROUTES.login} replace state={state} />
  }

  return <Outlet />
}
