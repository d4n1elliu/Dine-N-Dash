import { useState } from 'react'

interface FilterInputProps {
  id: string
  label: string
  value: string
  placeholder?: string
  // Called with the trimmed text when the user presses Enter or leaves the field
  onCommit: (value: string) => void
}

// A labelled text box for a filter row. Give it a key of its value so it resets when the value changes elsewhere
export default function FilterInput({ id, label, value, placeholder, onCommit }: FilterInputProps) {
  const [draft, setDraft] = useState(value)

  function commit() {
    const next = draft.trim()
    if (next !== value) onCommit(next)
  }

  return (
    <div className="flex items-center gap-3">
      <label htmlFor={id} className="text-sm font-medium">
        {label}
      </label>
      <input
        id={id}
        type="text"
        value={draft}
        placeholder={placeholder}
        onChange={(e) => setDraft(e.target.value)}
        onBlur={commit}
        onKeyDown={(e) => {
          if (e.key === 'Enter') commit()
        }}
        className="h-9 w-40 rounded border border-gray-300 bg-white px-3 text-sm focus:border-orange-500 focus:outline-none"
      />
    </div>
  )
}
