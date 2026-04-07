import { useState, useEffect } from 'react'
import { WeatherForecastDto, Selection, isTexasCity, REGIONS } from './types/weather'
import { RegionSelector } from './components/RegionSelector'
import { ForecastTable } from './components/ForecastTable'
import './App.css'

const API_BASE = import.meta.env.VITE_API_BASE ?? ''

function buildForecastUrl(selection: Selection): string {
  return isTexasCity(selection)
    ? `${API_BASE}/weather/city/${selection}`
    : `${API_BASE}/weather/${selection}`
}

function App() {
  const [selected, setSelected] = useState<Selection>(REGIONS[0])
  const [forecasts, setForecasts] = useState<WeatherForecastDto[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let cancelled = false
    setLoading(true)
    setError(null)

    fetch(buildForecastUrl(selected))
      .then(res => {
        if (!res.ok) throw new Error(`Request failed: ${res.status}`)
        return res.json() as Promise<WeatherForecastDto[]>
      })
      .then(data => {
        if (!cancelled) {
          setForecasts(data)
          setLoading(false)
        }
      })
      .catch((err: Error) => {
        if (!cancelled) {
          setError(err.message)
          setLoading(false)
        }
      })

    return () => { cancelled = true }
  }, [selected])

  return (
    <div className="app">
      <h1>DarkFactory Weather</h1>
      <RegionSelector selected={selected} onChange={setSelected} />
      {loading && <p>Loading…</p>}
      {error && <p className="error">Failed to load forecast: {error}</p>}
      {!loading && !error && <ForecastTable forecasts={forecasts} />}
    </div>
  )
}

export default App
