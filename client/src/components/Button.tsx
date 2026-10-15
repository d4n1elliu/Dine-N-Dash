import type { ButtonHTMLAttributes } from 'react'
import { buttonClass, type ButtonVariant } from '../lib/button-styles'

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: ButtonVariant
  fullWidth?: boolean
}

export default function Button({
  variant = 'primary',
  fullWidth = false,
  className = '',
  type = 'button',
  ...buttonProps
}: ButtonProps) {
  return <button type={type} className={`${buttonClass(variant, fullWidth)} ${className}`} {...buttonProps} />
}
