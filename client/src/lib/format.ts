const priceFormat = new Intl.NumberFormat('en-AU', { style: 'currency', currency: 'AUD' })

export function formatPrice(price: number): string {
  return priceFormat.format(price)
}
