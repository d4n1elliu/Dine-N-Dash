export const MIN_PASSWORD_LENGTH = 8

// Mirrors the backend rule so user@localhost is accepted
const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+$/

export function isValidEmail(value: string): boolean {
  return EMAIL_PATTERN.test(value)
}
