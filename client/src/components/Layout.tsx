import { startTransition } from 'react'
import { Link, NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/useAuth'
import { ROUTES } from '../routes'

const navLinkClass = ({ isActive }: { isActive: boolean }) =>
  `rounded px-3 py-2 text-sm font-medium transition ${
    isActive ? 'bg-orange-600 text-white' : 'text-gray-700 hover:bg-orange-100'
  }`

export default function Layout() {
  const { isAuthenticated, user, logout } = useAuth()
  const navigate = useNavigate()

  function handleLogout() {
    startTransition(() => {
      logout()
      navigate(ROUTES.home)
    })
  }

  return (
    <div className="flex min-h-screen flex-col bg-gray-50 text-gray-900">
      <header className="border-b border-gray-200 bg-white">
        <nav className="mx-auto flex max-w-5xl flex-wrap items-center justify-between gap-2 px-4 py-3">
          <Link to={ROUTES.home} className="text-xl font-bold text-orange-600">
            Dine N Dash
          </Link>
          <div className="flex items-center gap-1">
            <NavLink to={ROUTES.home} end className={navLinkClass}>
              Home
            </NavLink>
            <NavLink to={ROUTES.dashboard} className={navLinkClass}>
              Dashboard
            </NavLink>
            {isAuthenticated ? (
              <button
                type="button"
                onClick={handleLogout}
                className="rounded px-3 py-2 text-sm font-medium text-gray-700 hover:bg-orange-100"
              >
                Logout{user ? ` (${user.name})` : ''}
              </button>
            ) : (
              <NavLink to={ROUTES.login} className={navLinkClass}>
                Login
              </NavLink>
            )}
          </div>
        </nav>
      </header>
      <main className="mx-auto w-full max-w-5xl flex-1 px-4 py-8">
        <Outlet />
      </main>
    </div>
  )
}
