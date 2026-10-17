// Reads ?page= from the URL, falling back to 1 for a missing or invalid value
export function readPageParam(params: URLSearchParams): number {
  const page = Number(params.get('page'))
  return Number.isInteger(page) && page >= 1 ? page : 1
}
