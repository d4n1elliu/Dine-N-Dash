import { api } from './client'

export interface MenuItem {
  id: number
  menuCategoryId: number
  name: string
  description: string | null
  price: number
  imageUrl: string | null
}

export interface MenuCategory {
  id: number
  name: string
  items: MenuItem[]
}

export interface MenuItemInput {
  menuCategoryId: number
  name: string
  description: string | null
  price: number
}

export function getMenu(restaurantId: number): Promise<MenuCategory[]> {
  return api.get<MenuCategory[]>(`/api/menu/restaurant/${restaurantId}`)
}

export function createCategory(name: string): Promise<MenuCategory> {
  return api.post<MenuCategory>('/api/menu/categories', { name })
}

export function renameCategory(id: number, name: string): Promise<MenuCategory> {
  return api.put<MenuCategory>(`/api/menu/categories/${id}`, { name })
}

export function deleteCategory(id: number): Promise<void> {
  return api.delete<void>(`/api/menu/categories/${id}`)
}

export function createMenuItem(input: MenuItemInput): Promise<MenuItem> {
  return api.post<MenuItem>('/api/menu/items', input)
}

export function updateMenuItem(id: number, input: MenuItemInput): Promise<MenuItem> {
  return api.put<MenuItem>(`/api/menu/items/${id}`, input)
}

export function deleteMenuItem(id: number): Promise<void> {
  return api.delete<void>(`/api/menu/items/${id}`)
}

export function uploadMenuItemImage(id: number, file: File): Promise<MenuItem> {
  return api.upload<MenuItem>(`/api/menu/items/${id}/image`, file)
}

export function removeMenuItemImage(id: number): Promise<MenuItem> {
  return api.delete<MenuItem>(`/api/menu/items/${id}/image`)
}
