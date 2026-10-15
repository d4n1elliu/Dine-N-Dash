import type { UserRole } from '../api/auth'
import { ROLE_OPTIONS } from '../lib/roles'

interface RoleSelectProps {
  value: UserRole
  onChange: (role: UserRole) => void
  legend?: string
}

export default function RoleSelect({ value, onChange, legend = 'I want to' }: RoleSelectProps) {
  return (
    <fieldset>
      <legend className="mb-2 block text-sm font-medium">{legend}</legend>
      <div className="grid grid-cols-2 gap-2">
        {ROLE_OPTIONS.map((option) => {
          const selected = value === option.value
          return (
            <label
              key={option.value}
              className={`cursor-pointer rounded border px-3 py-2 text-sm transition focus-within:ring-2 focus-within:ring-orange-500 focus-within:ring-offset-1 ${
                selected
                  ? 'border-orange-600 bg-orange-50 ring-1 ring-orange-600'
                  : 'border-gray-300 hover:border-orange-300'
              }`}
            >
              <input
                type="radio"
                name="role"
                value={option.value}
                checked={selected}
                onChange={() => onChange(option.value)}
                className="sr-only"
              />
              <span className="block font-medium">{option.label}</span>
              <span className="block text-xs text-gray-600">{option.description}</span>
            </label>
          )
        })}
      </div>
    </fieldset>
  )
}
