import { Link, type LinkProps } from 'react-router-dom'

export default function TextLink({ className = '', ...linkProps }: LinkProps) {
  return <Link className={`text-orange-600 underline ${className}`} {...linkProps} />
}
