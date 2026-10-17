import { Navigate, Outlet } from 'react-router-dom'
import type { UserRole } from '../api/auth'
import { useAuth } from '../context/useAuth'
import { dashboardFor } from '../routes'

// Sends signed in users of the given role to their dashboard; everyone else, including guests, sees the page
export default function HideFromRole({ role }: { role: UserRole }) {
  const { user } = useAuth()

  if (user?.role === role) {
    return <Navigate to={dashboardFor(user.role)} replace />
  }

  return <Outlet />
}
