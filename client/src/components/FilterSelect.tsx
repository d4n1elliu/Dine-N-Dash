import type { ReactNode } from 'react'

interface FilterSelectProps {
  id: string
  label: string
  value: string
  onChange: (value: string) => void
  children: ReactNode
}

// A labelled dropdown for a filter row
export default function FilterSelect({ id, label, value, onChange, children }: FilterSelectProps) {
  return (
    <div className="flex items-center gap-3">
      <label htmlFor={id} className="text-sm font-medium">
        {label}
      </label>
      <div className="relative">
        <select
          id={id}
          value={value}
          onChange={(e) => onChange(e.target.value)}
          className="h-9 appearance-none rounded border border-gray-300 bg-white pr-9 pl-3 text-sm focus:border-orange-500 focus:outline-none"
        >
          {children}
        </select>
        {/* Replaces the browser's arrow, which sits against the edge and ignores padding */}
        <svg
          aria-hidden="true"
          viewBox="0 0 20 20"
          fill="none"
          stroke="currentColor"
          strokeWidth="1.5"
          strokeLinecap="round"
          strokeLinejoin="round"
          className="pointer-events-none absolute top-1/2 right-3 h-4 w-4 -translate-y-1/2 text-gray-600"
        >
          <path d="m6 8 4 4 4-4" />
        </svg>
      </div>
    </div>
  )
}
