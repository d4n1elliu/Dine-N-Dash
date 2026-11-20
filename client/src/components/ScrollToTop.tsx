import { useEffect, useRef } from 'react'
import { useLocation, useNavigationType } from 'react-router-dom'

// Starts each new page at the top. Render it once inside the router
export default function ScrollToTop() {
  const { pathname } = useLocation()
  const navigationType = useNavigationType()
  const lastPathname = useRef(pathname)

  useEffect(() => {
    // Only a new path counts, so changing a search filter doesn't jump the page
    if (lastPathname.current === pathname) return
    lastPathname.current = pathname

    // Back and forward are left alone so the browser can return to where you were
    if (navigationType !== 'POP') window.scrollTo({ top: 0 })
  }, [pathname, navigationType])

  return null
}
