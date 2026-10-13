import { api } from './client'

export interface HealthResponse {
  status: string
}

export function getHealth(): Promise<HealthResponse> {
  return api.get<HealthResponse>('/api/health')
}
