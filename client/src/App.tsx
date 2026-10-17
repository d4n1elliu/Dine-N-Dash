import { Route, Routes } from 'react-router-dom'
import HideFromRole from './components/HideFromRole'
import Layout from './components/Layout'
import ProtectedRoute from './components/ProtectedRoute'
import DashboardPage from './pages/DashboardPage'
import HomePage from './pages/HomePage'
import LoginPage from './pages/LoginPage'
import NotFoundPage from './pages/NotFoundPage'
import RegisterPage from './pages/RegisterPage'
import RestaurantDashboardPage from './pages/RestaurantDashboardPage'
import RestaurantPage from './pages/RestaurantPage'
import SearchPage from './pages/SearchPage'
import { ROUTES } from './routes'

function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route element={<HideFromRole role="RestaurantOwner" />}>
          <Route path={ROUTES.home} element={<HomePage />} />
        </Route>
        <Route path={ROUTES.search} element={<SearchPage />} />
        <Route path={ROUTES.restaurant} element={<RestaurantPage />} />
        <Route path={ROUTES.login} element={<LoginPage />} />
        <Route path={ROUTES.register} element={<RegisterPage />} />

        <Route element={<ProtectedRoute role="Diner" />}>
          <Route path={ROUTES.dashboard} element={<DashboardPage />} />
        </Route>

        <Route element={<ProtectedRoute role="RestaurantOwner" />}>
          <Route path={ROUTES.restaurantDashboard} element={<RestaurantDashboardPage />} />
        </Route>

        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  )
}

export default App
