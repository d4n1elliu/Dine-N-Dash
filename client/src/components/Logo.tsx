// The brand mark next to the site name. The image is decorative because the text already names the site
export default function Logo() {
  return (
    <span className="flex items-center gap-2 text-xl font-bold text-orange-600">
      <img src="/logo-mark.svg" alt="" width={32} height={32} className="h-8 w-8" />
      Dine N Dash
    </span>
  )
}
