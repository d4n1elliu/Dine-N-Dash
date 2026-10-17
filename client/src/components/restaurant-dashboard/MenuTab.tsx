import type { FormEvent } from 'react'
import { useState } from 'react'
import { createCategory, type MenuCategory } from '../../api/menu'
import type { ApiResult } from '../../hooks/useApi'
import { useSubmit } from '../../hooks/useSubmit'
import { inputClass, labelClass } from '../../lib/form-styles'
import Button from '../Button'
import FormError from '../FormError'
import MenuCategorySection from './MenuCategorySection'

// The menu is loaded by the dashboard page, which also uses it to tell whether setup is finished
export default function MenuTab({ menu }: { menu: ApiResult<MenuCategory[]> }) {
  const [name, setName] = useState('')
  const { submitting, error, setError, run } = useSubmit('Could not add the category. Please try again.')

  function handleAddCategory(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!name.trim()) {
      setError('Enter a category name, such as Entrees or Drinks.')
      return
    }
    void run(async () => {
      await createCategory(name.trim())
      setName('')
      menu.reload()
    })
  }

  if (menu.loading && !menu.data) return <p className="text-gray-500">Loading menu...</p>
  if (menu.error && !menu.data) return <p className="text-red-700">{menu.error}</p>

  const categories = menu.data ?? []

  return (
    <div className="space-y-6">
      {categories.map((category) => (
        <MenuCategorySection key={category.id} category={category} categories={categories} onChanged={menu.reload} />
      ))}

      <form onSubmit={handleAddCategory} noValidate className="space-y-3 rounded border border-gray-200 bg-white p-6">
        <label htmlFor="new-category" className={labelClass}>
          {categories.length === 0 ? 'Start with a category, such as Entrees or Mains' : 'Add another category'}
        </label>
        <div className="flex gap-2">
          <input
            id="new-category"
            value={name}
            maxLength={50}
            placeholder="Category name"
            onChange={(e) => setName(e.target.value)}
            className={inputClass}
          />
          <Button type="submit" disabled={submitting} className="shrink-0">
            {submitting ? 'Adding...' : 'Add category'}
          </Button>
        </div>
        <FormError message={error} />
      </form>
    </div>
  )
}
