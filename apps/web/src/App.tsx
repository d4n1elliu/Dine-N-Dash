import { useState } from 'react'
import './App.css'

function App() {
  const [count, setCount] = useState(0)

  return (
    <>
      <h1 className="text-center text-3xl font-bold">TABELOG CLONE WEBSITE</h1>

      <div className="ticks"></div>
      <section id="spacer"></section>
    </>
  )
}

export default App
