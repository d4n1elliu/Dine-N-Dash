import type { InputHTMLAttributes } from 'react'

interface FormFieldProps extends InputHTMLAttributes<HTMLInputElement> {
  id: string
  label: string
}

export default function FormField({ id, label, className = '', ...inputProps }: FormFieldProps) {
  return (
    <div>
      <label htmlFor={id} className="mb-1 block text-sm font-medium">
        {label}
      </label>
      <input
        id={id}
        className={`w-full rounded border border-gray-300 px-3 py-2 focus:border-orange-500 focus:outline-none ${className}`}
        {...inputProps}
      />
    </div>
  )
}
