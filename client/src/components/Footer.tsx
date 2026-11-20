const REPO_URL = 'https://github.com/d4n1elliu/Dine-N-Dash'

export default function Footer() {
  return (
    <footer className="border-t border-gray-200 bg-white">
      <p className="mx-auto max-w-5xl px-4 py-4 text-center text-sm text-gray-600">
        Dine N Dash · A university group project ·{' '}
        <a href={REPO_URL} target="_blank" rel="noreferrer" className="text-orange-600 underline">
          GitHub
        </a>
      </p>
    </footer>
  )
}
