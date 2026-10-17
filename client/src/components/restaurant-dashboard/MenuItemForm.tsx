import type { FormEvent } from 'react'
import { useState } from 'react'
import type { MenuCategory, MenuItem, MenuItemInput } from '../../api/menu'
import { useSubmit } from '../../hooks/useSubmit'
import { inputClass, labelClass, outlineButtonClass } from '../../lib/form-styles'
import Button from '../Button'
import FormError from '../FormError'

interface MenuItemFormProps {
  categories: MenuCategory[]
  categoryId: number
  item?: MenuItem
  onSubmit: (input: MenuItemInput) => Promise<void>
  onCancel?: () => void
}

// Adds an item to a category, or edits one; editing also lets the item move to another category
export default function MenuItemForm({ categories, categoryId, item, onSubmit, onCancel }: MenuItemFormProps) {
  const [name, setName] = useState(item?.name ?? '')
  const [description, setDescription] = useState(item?.description ?? '')
  const [price, setPrice] = useState(item ? String(item.price) : '')
  const [menuCategoryId, setMenuCategoryId] = useState(item?.menuCategoryId ?? categoryId)
  const { submitting, error, setError, run } = useSubmit('Could not save the menu item. Please try again.')
  const idPrefix = item ? `item-${item.id}` : `new-item-${categoryId}`

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    const parsedPrice = Number(price)
    if (!name.trim()) {
      setError('Enter a name for the item.')
      return
    }
    if (price.trim() === '' || !Number.isFinite(parsedPrice) || parsedPrice < 0) {
      setError('Enter a price of 0 or more.')
      return
    }

    void run(async () => {
      await onSubmit({
        menuCategoryId,
        name: name.trim(),
        description: description.trim() || null,
        price: Math.round(parsedPrice * 100) / 100,
      })
      if (!item) {
        setName('')
        setDescription('')
        setPrice('')
      }
    })
  }

  return (
    <form onSubmit={handleSubmit} noValidate className="space-y-3 rounded border border-gray-200 bg-gray-50 p-4">
      <div className="grid gap-3 sm:grid-cols-[1fr_8rem]">
        <div>
          <label htmlFor={`${idPrefix}-name`} className={labelClass}>
            Item name
          </label>
          <input
            id={`${idPrefix}-name`}
            value={name}
            maxLength={100}
            onChange={(e) => setName(e.target.value)}
            className={inputClass}
          />
        </div>
        <div>
          <label htmlFor={`${idPrefix}-price`} className={labelClass}>
            Price ($)
          </label>
          <input
            id={`${idPrefix}-price`}
            type="number"
            inputMode="decimal"
            min={0}
            step="0.01"
            value={price}
            onChange={(e) => setPrice(e.target.value)}
            className={inputClass}
          />
        </div>
      </div>
      <div>
        <label htmlFor={`${idPrefix}-description`} className={labelClass}>
          Description <span className="font-normal text-gray-500">(optional)</span>
        </label>
        <input
          id={`${idPrefix}-description`}
          value={description}
          maxLength={500}
          onChange={(e) => setDescription(e.target.value)}
          className={inputClass}
        />
      </div>
      {item && categories.length > 1 && (
        <div>
          <label htmlFor={`${idPrefix}-category`} className={labelClass}>
            Category
          </label>
          <select
            id={`${idPrefix}-category`}
            value={menuCategoryId}
            onChange={(e) => setMenuCategoryId(Number(e.target.value))}
            className={inputClass}
          >
            {categories.map((category) => (
              <option key={category.id} value={category.id}>
                {category.name}
              </option>
            ))}
          </select>
        </div>
      )}
      <FormError message={error} />
      <div className="flex gap-2">
        <Button type="submit" disabled={submitting}>
          {submitting ? 'Saving...' : item ? 'Save item' : 'Add item'}
        </Button>
        {onCancel && (
          <button type="button" className={outlineButtonClass} onClick={onCancel}>
            Cancel
          </button>
        )}
      </div>
    </form>
  )
}
