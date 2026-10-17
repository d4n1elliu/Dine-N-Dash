import type { ChangeEvent } from 'react'
import { useRef } from 'react'
import { apiUrl } from '../api/client'
import { useSubmit } from '../hooks/useSubmit'
import { outlineButtonClass } from '../lib/form-styles'
import FormError from './FormError'

const MAX_SIZE_BYTES = 5 * 1024 * 1024
const ACCEPTED_TYPES = ['image/jpeg', 'image/png', 'image/webp']

interface ImageUploadProps {
  imageUrl: string | null
  alt: string
  onUpload: (file: File) => Promise<void>
  onRemove: () => Promise<void>
  size?: 'large' | 'small'
}

// Picks a file and uploads it straight away; the backend replaces and deletes any previous image
export default function ImageUpload({ imageUrl, alt, onUpload, onRemove, size = 'large' }: ImageUploadProps) {
  const inputRef = useRef<HTMLInputElement>(null)
  const { submitting, error, setError, run } = useSubmit('Could not update the image. Please try again.')

  function handleFileChange(event: ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0]
    event.target.value = ''
    if (!file) return

    if (!ACCEPTED_TYPES.includes(file.type)) {
      setError('Choose a JPEG, PNG or WebP image.')
      return
    }
    if (file.size > MAX_SIZE_BYTES) {
      setError('Images must be 5 MB or smaller.')
      return
    }

    void run(() => onUpload(file))
  }

  const previewClass = size === 'large' ? 'h-40 w-64' : 'h-16 w-16'

  return (
    <div className="space-y-2">
      <div className="flex flex-wrap items-center gap-3">
        {imageUrl ? (
          <img src={apiUrl(imageUrl)} alt={alt} className={`${previewClass} rounded border border-gray-200 object-cover`} />
        ) : (
          <div
            className={`${previewClass} flex items-center justify-center rounded border border-dashed border-gray-300 bg-gray-50 text-xs text-gray-500`}
          >
            No image
          </div>
        )}
        <div className="flex flex-wrap gap-2">
          <button
            type="button"
            className={outlineButtonClass}
            disabled={submitting}
            onClick={() => inputRef.current?.click()}
          >
            {submitting ? 'Saving...' : imageUrl ? 'Replace image' : 'Upload image'}
          </button>
          {imageUrl && (
            <button type="button" className={outlineButtonClass} disabled={submitting} onClick={() => void run(onRemove)}>
              Remove
            </button>
          )}
        </div>
        <input
          ref={inputRef}
          type="file"
          accept={ACCEPTED_TYPES.join(',')}
          onChange={handleFileChange}
          className="sr-only"
          tabIndex={-1}
          aria-label={`Upload ${alt}`}
        />
      </div>
      <FormError message={error} />
    </div>
  )
}
