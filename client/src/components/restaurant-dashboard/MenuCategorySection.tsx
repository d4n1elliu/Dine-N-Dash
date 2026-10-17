import type { FormEvent } from 'react'
import { useState } from 'react'
import { apiUrl } from '../../api/client'
import {
  createMenuItem,
  deleteCategory,
  deleteMenuItem,
  removeMenuItemImage,
  renameCategory,
  updateMenuItem,
  uploadMenuItemImage,
  type MenuCategory,
  type MenuItem,
} from '../../api/menu'
import { useSubmit } from '../../hooks/useSubmit'
import { dangerButtonClass, inputClass, outlineButtonClass } from '../../lib/form-styles'
import Button from '../Button'
import FormError from '../FormError'
import ImageUpload from '../ImageUpload'
import MenuItemForm from './MenuItemForm'

interface MenuCategorySectionProps {
  category: MenuCategory
  categories: MenuCategory[]
  onChanged: () => void
}

const priceFormat = new Intl.NumberFormat('en-AU', { style: 'currency', currency: 'AUD' })

export default function MenuCategorySection({ category, categories, onChanged }: MenuCategorySectionProps) {
  const [renaming, setRenaming] = useState(false)
  const [newName, setNewName] = useState(category.name)
  const [editingItemId, setEditingItemId] = useState<number | null>(null)
  const [adding, setAdding] = useState(category.items.length === 0)
  const { submitting, error, setError, run } = useSubmit('Could not update the category. Please try again.')

  function handleRename(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!newName.trim()) {
      setError('Enter a category name.')
      return
    }
    void run(async () => {
      await renameCategory(category.id, newName.trim())
      setRenaming(false)
      onChanged()
    })
  }

  function handleDeleteCategory() {
    const itemCount = category.items.length
    const message = itemCount
      ? `Delete "${category.name}" and its ${itemCount} ${itemCount === 1 ? 'item' : 'items'}?`
      : `Delete "${category.name}"?`
    if (!window.confirm(message)) return

    void run(async () => {
      await deleteCategory(category.id)
      onChanged()
    })
  }

  function handleDeleteItem(item: MenuItem) {
    if (!window.confirm(`Delete "${item.name}"?`)) return
    void run(async () => {
      await deleteMenuItem(item.id)
      onChanged()
    })
  }

  return (
    <section className="space-y-4 rounded border border-gray-200 bg-white p-6">
      <div className="flex flex-wrap items-center justify-between gap-2">
        {renaming ? (
          <form onSubmit={handleRename} className="flex flex-1 flex-wrap gap-2">
            <label htmlFor={`category-${category.id}-name`} className="sr-only">
              Category name
            </label>
            <input
              id={`category-${category.id}-name`}
              value={newName}
              maxLength={50}
              onChange={(e) => setNewName(e.target.value)}
              className={`${inputClass} max-w-xs`}
            />
            <Button type="submit" disabled={submitting}>
              Save
            </Button>
            <button
              type="button"
              className={outlineButtonClass}
              onClick={() => {
                setRenaming(false)
                setNewName(category.name)
                setError(null)
              }}
            >
              Cancel
            </button>
          </form>
        ) : (
          <>
            <h2 className="text-lg font-semibold">{category.name}</h2>
            <div className="flex gap-2">
              <button type="button" className={outlineButtonClass} onClick={() => setRenaming(true)}>
                Rename
              </button>
              <button type="button" className={dangerButtonClass} disabled={submitting} onClick={handleDeleteCategory}>
                Delete
              </button>
            </div>
          </>
        )}
      </div>
      <FormError message={error} />

      {category.items.length > 0 && (
        <ul className="divide-y divide-gray-100">
          {category.items.map((item) =>
            editingItemId === item.id ? (
              <li key={item.id} className="space-y-3 py-3">
                <ImageUpload
                  size="small"
                  imageUrl={item.imageUrl}
                  alt={`${item.name} image`}
                  onUpload={async (file) => {
                    await uploadMenuItemImage(item.id, file)
                    onChanged()
                  }}
                  onRemove={async () => {
                    await removeMenuItemImage(item.id)
                    onChanged()
                  }}
                />
                <MenuItemForm
                  categories={categories}
                  categoryId={category.id}
                  item={item}
                  onSubmit={async (input) => {
                    await updateMenuItem(item.id, input)
                    setEditingItemId(null)
                    onChanged()
                  }}
                  onCancel={() => setEditingItemId(null)}
                />
              </li>
            ) : (
              <li key={item.id} className="flex flex-wrap items-center gap-3 py-3">
                <MenuItemThumbnail item={item} />
                <div className="min-w-0 flex-1">
                  <p className="font-medium">{item.name}</p>
                  {item.description && <p className="text-sm text-gray-600">{item.description}</p>}
                </div>
                <p className="font-medium">{priceFormat.format(item.price)}</p>
                <div className="flex gap-2">
                  <button type="button" className={outlineButtonClass} onClick={() => setEditingItemId(item.id)}>
                    Edit
                  </button>
                  <button type="button" className={dangerButtonClass} onClick={() => handleDeleteItem(item)}>
                    Delete
                  </button>
                </div>
              </li>
            ),
          )}
        </ul>
      )}

      {adding ? (
        <MenuItemForm
          categories={categories}
          categoryId={category.id}
          onSubmit={async (input) => {
            await createMenuItem(input)
            onChanged()
          }}
          onCancel={category.items.length > 0 ? () => setAdding(false) : undefined}
        />
      ) : (
        <button type="button" className={outlineButtonClass} onClick={() => setAdding(true)}>
          + Add item to {category.name}
        </button>
      )}
    </section>
  )
}

function MenuItemThumbnail({ item }: { item: MenuItem }) {
  if (!item.imageUrl) {
    return <div className="h-12 w-12 shrink-0 rounded bg-orange-50" aria-hidden="true" />
  }
  return <img src={apiUrl(item.imageUrl)} alt="" className="h-12 w-12 shrink-0 rounded object-cover" />
}
