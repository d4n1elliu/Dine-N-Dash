import type { MenuCategory } from '../../api/menu'
import { formatPrice } from '../../lib/format'

interface PreOrderPickerProps {
  menu: MenuCategory[]
  quantities: Record<number, number>
  onChange: (itemId: number, delta: number) => void
}

// Let diner choose dishes ahead of time, show nothing if the restaurant has no menu items yet
export default function PreOrderPicker({ menu, quantities, onChange }: PreOrderPickerProps) {
  const categories = menu.filter((category) => category.items.length > 0)
  if (categories.length === 0) return null

  return (
    <fieldset className="space-y-3">
      <legend className="text-sm font-medium">Pre-order dishes (optional)</legend>
      {categories.map((category) => (
        <div key={category.id}>
          <h2 className="text-sm font-semibold text-gray-700">{category.name}</h2>
          <ul className="divide-y divide-gray-100">
            {category.items.map((item) => (
              <li key={item.id} className="flex items-center gap-3 py-2">
                <div className="min-w-0 flex-1">
                  <p className="text-sm font-medium">{item.name}</p>
                  <p className="text-xs text-gray-600">{formatPrice(item.price)}</p>
                </div>
                <div className="flex items-center gap-2">
                  <button
                    type="button"
                    aria-label={`Remove one ${item.name}`}
                    disabled={(quantities[item.id] ?? 0) === 0}
                    onClick={() => onChange(item.id, -1)}
                    className="rounded border border-gray-300 px-2 py-1 text-sm disabled:opacity-40"
                  >
                    −
                  </button>
                  <span className="w-6 text-center text-sm">{quantities[item.id] ?? 0}</span>
                  <button
                    type="button"
                    aria-label={`Add one ${item.name}`}
                    onClick={() => onChange(item.id, 1)}
                    className="rounded border border-gray-300 px-2 py-1 text-sm"
                  >
                    +
                  </button>
                </div>
              </li>
            ))}
          </ul>
        </div>
      ))}
    </fieldset>
  )
}