import { getRecommendedRestaurants } from '../api/restaurants'
import GuestRecommended from '../components/GuestRecommended'
import HomeHero from '../components/HomeHero'
import PagedRecommended from '../components/PagedRecommended'
import { useAuth } from '../context/useAuth'
import { HERO_PHOTO_COUNT } from '../lib/home-hero'

const GUEST_CARD_COUNT = 6

const fetchTopPhotos = async () => (await getRecommendedRestaurants(1, HERO_PHOTO_COUNT)).items
// The page after the guest cards, so the hero never repeats a restaurant shown below it
const fetchPhotosAfterGuestCards = async () => (await getRecommendedRestaurants(2, GUEST_CARD_COUNT)).items

// Guests get a short landing page; signed in customers get the full paginated list
export default function HomePage() {
  const { isAuthenticated } = useAuth()

  return (
    <section className="space-y-8">
      {isAuthenticated ? (
        <>
          <HomeHero key="customer" fetchPhotos={fetchTopPhotos} />
          <PagedRecommended />
        </>
      ) : (
        <>
          <HomeHero key="guest" fetchPhotos={fetchPhotosAfterGuestCards} />
          <GuestRecommended count={GUEST_CARD_COUNT} />
        </>
      )}
    </section>
  )
}
