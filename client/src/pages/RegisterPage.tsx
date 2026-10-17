import type { ChangeEvent, FormEvent } from 'react'
import { useState } from 'react'
import { Navigate, useLocation, useNavigate } from 'react-router-dom'
import type { UserRole } from '../api/auth'
import Button from '../components/Button'
import CheckList from '../components/CheckList'
import FormError from '../components/FormError'
import FormField from '../components/FormField'
import RoleSelect from '../components/RoleSelect'
import TextLink from '../components/TextLink'
import { useAuth } from '../context/useAuth'
import { useSubmit } from '../hooks/useSubmit'
import { ROLE_BENEFITS } from '../lib/roles'
import { isValidEmail, MIN_PASSWORD_LENGTH } from '../lib/validation'
import { homeFor, ROUTES, type RedirectState } from '../routes'

export default function RegisterPage() {
  const { register, isAuthenticated, user } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  // Set when the user arrived here from a page they should return to, such as a restaurant's Book button
  const returnTo = (location.state as RedirectState | null)?.from
  const [name, setName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [role, setRole] = useState<UserRole>('Diner')
  const { submitting, error, setError, run } = useSubmit('Registration failed. Please try again.')

  // Clears the error on edit so it never describes a value the user has since changed
  const onFieldChange =
    (setValue: (value: string) => void) => (event: ChangeEvent<HTMLInputElement>) => {
      setValue(event.target.value)
      setError(null)
    }

  if (isAuthenticated) {
    return <Navigate to={returnTo ?? homeFor(user?.role)} replace />
  }

  function validate(): string | null {
    if (name.trim().length === 0) return 'Enter your name.'
    if (!isValidEmail(email)) return 'Enter a valid email address.'
    if (password.length < MIN_PASSWORD_LENGTH) {
      return `Password must be at least ${MIN_PASSWORD_LENGTH} characters.`
    }
    if (password !== confirmPassword) return 'Passwords do not match.'
    return null
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    const validationError = validate()
    if (validationError) {
      setError(validationError)
      return
    }

    // register throws if a newer auth attempt superseded this one, which skips the navigate
    void run(async () => {
      const signedIn = await register(name.trim(), email, password, role)
      navigate(returnTo ?? homeFor(signedIn.role), { replace: true })
    })
  }

  return (
    <section className="grid items-center gap-10 py-4 lg:grid-cols-2 lg:gap-16 lg:py-12">
      <div className="space-y-6">
        <h1 className="text-4xl font-bold tracking-tight sm:text-5xl">
          Your next meal starts with Dine N Dash
        </h1>
        <p className="text-lg text-gray-600">
          Create a free account to search restaurants, book tables and share reviews, or list your
          own restaurant and start taking bookings.
        </p>
        <CheckList items={ROLE_BENEFITS[role]} />
      </div>

      <form
        onSubmit={handleSubmit}
        noValidate
        className="w-full max-w-md space-y-4 rounded-lg border border-gray-200 bg-white p-6 shadow-sm sm:p-8 lg:justify-self-end"
      >
        <h2 className="text-2xl font-bold">Sign up</h2>
        <RoleSelect value={role} onChange={setRole} />
        <FormField
          id="name"
          label="Name"
          type="text"
          autoComplete="name"
          placeholder="Jane Smith"
          value={name}
          onChange={onFieldChange(setName)}
        />
        <FormField
          id="email"
          label="Email"
          type="email"
          autoComplete="email"
          placeholder="you@example.com"
          value={email}
          onChange={onFieldChange(setEmail)}
        />
        <div className="grid gap-4 sm:grid-cols-2">
          <FormField
            id="password"
            label="Password"
            type="password"
            autoComplete="new-password"
            value={password}
            onChange={onFieldChange(setPassword)}
          />
          <FormField
            id="confirm-password"
            label="Confirm password"
            type="password"
            autoComplete="new-password"
            value={confirmPassword}
            onChange={onFieldChange(setConfirmPassword)}
          />
        </div>
        <p className="text-xs text-gray-500">Use at least {MIN_PASSWORD_LENGTH} characters.</p>
        <FormError message={error} />
        <Button type="submit" fullWidth disabled={submitting} className="py-3 font-semibold">
          {submitting ? 'Creating your account...' : 'Get started for free'}
        </Button>
        <p className="text-center text-sm text-gray-600">
          Already have an account? <TextLink to={ROUTES.login} state={location.state}>
            Log in
          </TextLink>
        </p>
      </form>
    </section>
  )
}
