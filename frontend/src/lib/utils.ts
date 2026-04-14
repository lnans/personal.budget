import { clsx, type ClassValue } from 'clsx'
import { twMerge } from 'tailwind-merge'

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

/**
 * Decodes a JWT token and returns the payload
 * @param token - The JWT token string
 * @returns The decoded payload or null if invalid
 */
function decodeJwt(token: string): Record<string, unknown> | null {
  try {
    const parts = token.split('.')
    if (parts.length !== 3) {
      return null
    }

    const payload = parts[1]
    const decoded = atob(payload.replace(/-/g, '+').replace(/_/g, '/'))
    return JSON.parse(decoded) as Record<string, unknown>
  } catch {
    return null
  }
}

/**
 * Gets the expiration time from a JWT token
 * @param token - The JWT token string
 * @returns The expiration date or null if invalid/not found
 */
export function getTokenExpiration(token: string | null): Date | null {
  if (!token) {
    return null
  }

  const payload = decodeJwt(token)
  if (!payload || typeof payload.exp !== 'number') {
    return null
  }

  // exp is in seconds, Date constructor expects milliseconds
  return new Date(payload.exp * 1000)
}

/**
 * Checks if a JWT token is expired
 * @param token - The JWT token string
 * @returns true if expired or invalid, false if still valid
 */
export function isTokenExpired(token: string | null): boolean {
  const exp = getTokenExpiration(token)
  if (!exp) {
    return true
  }

  return Date.now() >= exp.getTime()
}

/**
 * Formats a currency amount to a string
 * @param amount - The amount to format
 * @param currency - The currency to format (default: 'EUR')
 * @param locale - The locale to format the currency in (default: 'fr-FR')
 * @returns The formatted currency amount
 */
export function formatCurrency(amount: number, currency = 'EUR', locale = 'fr-FR'): string {
  return new Intl.NumberFormat(locale, {
    style: 'currency',
    currency,
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
    currencyDisplay: 'symbol',
  }).format(amount)
}

/**
 * Returns the color of an account currency amount
 * @param amount - The amount to get the color for
 * @returns The color of the amount
 */
export function cnAccountCurrencyColor(amount: number): string {
  if (amount === 0) {
    return 'text-gray-500'
  }
  if (amount < 0) {
    return 'text-red-500'
  }
  return ''
}

/**
 * Returns the color of an operation currency amount
 * @param amount - The amount to get the color for
 * @returns The color of the amount
 */
export function cnOperationCurrencyColor(amount: number): string {
  if (amount === 0) {
    return 'text-gray-500'
  }
  if (amount > 0) {
    return 'text-green-500'
  }
  return ''
}

export function formatDate(
  date: Date | string | null | undefined,
  format: 'short' | 'medium' | 'long' | 'full' = 'medium',
  locale = 'fr-FR'
): string {
  if (!date) {
    return ''
  }

  const dateObj = typeof date === 'string' ? new Date(date) : date

  return new Intl.DateTimeFormat(locale, {
    dateStyle: format,
  }).format(dateObj)
}
