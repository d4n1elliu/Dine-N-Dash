export type ButtonVariant = 'primary' | 'pill'

const variantClasses: Record<ButtonVariant, string> = {
  primary: 'rounded px-4 py-2 font-medium',
  pill: 'rounded-full px-4 py-2 text-sm font-semibold',
}

// Kept outside Button.tsx so react-router Links can look like buttons without a polymorphic component
export function buttonClass(variant: ButtonVariant = 'primary', fullWidth = false): string {
  return `${variantClasses[variant]} bg-orange-600 text-white transition hover:bg-orange-700 disabled:opacity-60 ${
    fullWidth ? 'w-full' : ''
  }`
}
