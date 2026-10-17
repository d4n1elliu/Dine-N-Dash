import { getStoredToken } from '../context/session-storage'

const baseUrl = (import.meta.env.VITE_API_URL ?? '').replace(/\/+$/, '')

export class ApiError extends Error {
  status: number

  constructor(status: number, message: string) {
    super(message)
    this.name = 'ApiError'
    this.status = status
  }
}

// Image and other links from the API are relative, so they need the API's address in front
export function apiUrl(path: string): string {
  return `${baseUrl}${path}`
}

export function getErrorMessage(error: unknown, fallback = 'Something went wrong.'): string {
  return error instanceof ApiError ? error.message : fallback
}

type RequestOptions = Omit<RequestInit, 'body'> & { body?: unknown }

export async function apiFetch<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const { body, headers, ...rest } = options
  const requestHeaders = new Headers(headers)
  requestHeaders.set('Accept', 'application/json')

  // The browser sets the multipart Content-Type and boundary itself for FormData
  const isForm = body instanceof FormData
  if (body !== undefined && !isForm) {
    requestHeaders.set('Content-Type', 'application/json')
  }

  const token = getStoredToken()
  if (token) {
    requestHeaders.set('Authorization', `Bearer ${token}`)
  }

  let response: Response
  try {
    response = await fetch(`${baseUrl}${path}`, {
      ...rest,
      headers: requestHeaders,
      body: isForm ? body : body !== undefined ? JSON.stringify(body) : undefined,
    })
  } catch {
    throw new ApiError(0, 'Could not reach the server. Is the API running?')
  }

  if (!response.ok) {
    throw new ApiError(response.status, await readErrorMessage(response))
  }

  if (response.status === 204) {
    return undefined as T
  }

  return (await response.json()) as T
}

async function readErrorMessage(response: Response): Promise<string> {
  try {
    const problem = (await response.json()) as { detail?: string; title?: string }
    return problem.detail || problem.title || `Request failed with status ${response.status}`
  } catch {
    return `Request failed with status ${response.status}`
  }
}

export const api = {
  get: <T>(path: string) => apiFetch<T>(path),
  post: <T>(path: string, body?: unknown) => apiFetch<T>(path, { method: 'POST', body }),
  put: <T>(path: string, body?: unknown) => apiFetch<T>(path, { method: 'PUT', body }),
  delete: <T>(path: string) => apiFetch<T>(path, { method: 'DELETE' }),
  // Sends one file as multipart form data in a field named file
  upload: <T>(path: string, file: File) => {
    const form = new FormData()
    form.append('file', file)
    return apiFetch<T>(path, { method: 'PUT', body: form })
  },
}
