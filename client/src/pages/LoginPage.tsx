import type { FormEvent } from 'react'
import { useState } from 'react'
import { Navigate, useLocation, useNavigate } from 'react-router-dom'
import Button from '../components/Button'
import FormError from '../components/FormError'
import FormField from '../components/FormField'
import TextLink from '../components/TextLink'
import { useAuth } from '../context/useAuth'
import { useSubmit } from '../hooks/useSubmit'
import { homeFor, ROUTES, type RedirectState } from '../routes'

export default function LoginPage() {
  const { login, isAuthenticated, user } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const { submitting, error, setError, run } = useSubmit('Login failed. Please try again.')

  const returnTo = (location.state as RedirectState | null)?.from

  if (isAuthenticated) {
    return <Navigate to={returnTo ?? homeFor(user?.role)} replace />
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    if (email.trim().length === 0) {
      setError('Enter your email address.')
      return
    }
    if (password.length === 0) {
      setError('Enter your password.')
      return
    }

    void run(async () => {
      const signedIn = await login(email, password)
      navigate(returnTo ?? homeFor(signedIn.role), { replace: true })
    })
  }

  return (
    <section className="mx-auto max-w-sm">
      <h1 className="mb-6 text-2xl font-bold">Log in</h1>
      <form onSubmit={handleSubmit} noValidate className="space-y-4 rounded border border-gray-200 bg-white p-6">
        <FormField
          id="email"
          label="Email"
          type="email"
          autoComplete="email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
        />
        <FormField
          id="password"
          label="Password"
          type="password"
          autoComplete="current-password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
        />
        <FormError message={error} />
        <Button type="submit" fullWidth disabled={submitting}>
          {submitting ? 'Logging in...' : 'Log in'}
        </Button>
      </form>
      <p className="mt-4 text-center text-sm text-gray-600">
        Don&apos;t have an account? <TextLink to={ROUTES.register} state={location.state}>
          Sign up
        </TextLink>
      </p>
    </section>
  )
}
