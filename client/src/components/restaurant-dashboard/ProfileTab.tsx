import type { FormEvent } from 'react'
import { useState } from 'react'
import {
  createMyRestaurant,
  CUISINES,
  cuisineLabel,
  removeRestaurantImage,
  updateMyRestaurant,
  uploadRestaurantImage,
  type RestaurantProfile,
  type RestaurantProfileInput,
} from '../../api/restaurants'
import { useSubmit } from '../../hooks/useSubmit'
import { inputClass, labelClass } from '../../lib/form-styles'
import Button from '../Button'
import FormError from '../FormError'
import FormField from '../FormField'
import ImageUpload from '../ImageUpload'

interface ProfileTabProps {
  profile: RestaurantProfile | null
  onSaved: (profile: RestaurantProfile) => void
}

const emptyInput: RestaurantProfileInput = {
  name: '',
  description: '',
  cuisine: CUISINES.indexOf('Other'),
  address: '',
  suburb: '',
  priceLevel: 2,
  capacity: 20,
}

function toInput(profile: RestaurantProfile): RestaurantProfileInput {
  const { name, description, cuisine, address, suburb, priceLevel, capacity } = profile
  return { name, description, cuisine, address, suburb, priceLevel, capacity }
}

// Creates the restaurant the first time, then edits it; the image can only be added once the restaurant exists
export default function ProfileTab({ profile, onSaved }: ProfileTabProps) {
  const [input, setInput] = useState<RestaurantProfileInput>(profile ? toInput(profile) : emptyInput)
  const [saved, setSaved] = useState(false)
  const { submitting, error, setError, run } = useSubmit('Could not save your restaurant. Please try again.')
  const isNew = profile === null

  function update<K extends keyof RestaurantProfileInput>(key: K, value: RestaurantProfileInput[K]) {
    setInput((current) => ({ ...current, [key]: value }))
    setSaved(false)
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    if (!input.name.trim()) return setError('Enter your restaurant name.')
    if (!input.address.trim()) return setError('Enter your address.')
    if (!input.suburb.trim()) return setError('Enter your suburb.')
    if (!Number.isInteger(input.capacity) || input.capacity < 1) return setError('Capacity must be at least 1.')

    void run(async () => {
      const result = isNew ? await createMyRestaurant(input) : await updateMyRestaurant(input)
      setSaved(true)
      onSaved(result)
    })
  }

  return (
    <div className="space-y-6">
      {profile && (
        <section className="space-y-3 rounded border border-gray-200 bg-white p-6">
          <h2 className="font-semibold">Profile image</h2>
          <p className="text-sm text-gray-600">Shown on your restaurant card. JPEG, PNG or WebP, up to 5 MB.</p>
          <ImageUpload
            imageUrl={profile.profileImageUrl}
            alt={`${profile.name} profile image`}
            onUpload={async (file) => onSaved(await uploadRestaurantImage(file))}
            onRemove={async () => onSaved(await removeRestaurantImage())}
          />
        </section>
      )}

      <form onSubmit={handleSubmit} noValidate className="space-y-4 rounded border border-gray-200 bg-white p-6">
        <h2 className="font-semibold">{isNew ? 'Restaurant details' : 'Edit details'}</h2>
        <FormField id="name" label="Restaurant name" value={input.name} maxLength={100} onChange={(e) => update('name', e.target.value)} />
        <div>
          <label htmlFor="description" className={labelClass}>
            Description
          </label>
          <textarea
            id="description"
            rows={3}
            maxLength={1000}
            value={input.description}
            onChange={(e) => update('description', e.target.value)}
            className={inputClass}
          />
        </div>
        <div className="grid gap-4 sm:grid-cols-2">
          <FormField id="address" label="Address" value={input.address} onChange={(e) => update('address', e.target.value)} />
          <FormField id="suburb" label="Suburb" value={input.suburb} onChange={(e) => update('suburb', e.target.value)} />
        </div>
        <div className="grid gap-4 sm:grid-cols-3">
          <div>
            <label htmlFor="cuisine" className={labelClass}>
              Cuisine
            </label>
            <select
              id="cuisine"
              value={input.cuisine}
              onChange={(e) => update('cuisine', Number(e.target.value))}
              className={inputClass}
            >
              {CUISINES.map((cuisine, index) => (
                <option key={cuisine} value={index}>
                  {cuisineLabel(cuisine)}
                </option>
              ))}
            </select>
          </div>
          <div>
            <label htmlFor="priceLevel" className={labelClass}>
              Price level
            </label>
            <select
              id="priceLevel"
              value={input.priceLevel}
              onChange={(e) => update('priceLevel', Number(e.target.value))}
              className={inputClass}
            >
              {[1, 2, 3, 4].map((level) => (
                <option key={level} value={level}>
                  {'$'.repeat(level)}
                </option>
              ))}
            </select>
          </div>
          <FormField
            id="capacity"
            label="Seats"
            type="number"
            min={1}
            value={Number.isNaN(input.capacity) ? '' : input.capacity}
            onChange={(e) => update('capacity', e.target.valueAsNumber)}
          />
        </div>
        <FormError message={error} />
        {saved && !error && (
          <p role="status" className="text-sm text-green-700">
            Saved.
          </p>
        )}
        <Button type="submit" disabled={submitting}>
          {submitting ? 'Saving...' : isNew ? 'Create restaurant' : 'Save changes'}
        </Button>
      </form>
    </div>
  )
}
