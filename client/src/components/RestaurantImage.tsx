import { useState } from 'react'
import { apiUrl } from '../api/client'

interface RestaurantImageProps {
  imageUrl: string | null
  name: string
  className: string
  alt?: string
}

// Shows the restaurant's photo, or its first letter when there is no photo or the file fails to load
export default function RestaurantImage({ imageUrl, name, className, alt = name }: RestaurantImageProps) {
  const [failedUrl, setFailedUrl] = useState<string | null>(null)

  if (imageUrl && imageUrl !== failedUrl) {
    return (
      <img
        src={apiUrl(imageUrl)}
        alt={alt}
        className={`${className} w-full object-cover`}
        onError={() => setFailedUrl(imageUrl)}
      />
    )
  }

  return (
    <div
      aria-hidden="true"
      className={`${className} flex items-center justify-center bg-orange-50 font-bold text-orange-300`}
    >
      {Array.from(name)[0] ?? ''}
    </div>
  )
}
