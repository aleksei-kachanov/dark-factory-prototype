import { useEffect, useState } from 'react';
import { RegionSelector } from './components/RegionSelector';
import { ForecastTable } from './components/ForecastTable';
import type { Region, WeatherForecastDto } from './types/weather';
import './App.css';

function App() {
  const [region, setRegion] = useState<Region>('temperate');
  const [fetchedRegion, setFetchedRegion] = useState<Region | null>(null);
  const [forecasts, setForecasts] = useState<WeatherForecastDto[]>([]);
  const [error, setError] = useState<string | null>(null);

  const loading = fetchedRegion !== region;

  useEffect(() => {
    let cancelled = false;
    fetch(`/api/v1/weather/${region}`)
      .then((res) => {
        if (!res.ok) throw new Error(`Request failed: ${res.status} ${res.statusText}`);
        return res.json() as Promise<WeatherForecastDto[]>;
      })
      .then((data) => {
        if (!cancelled) {
          setForecasts(data);
          setError(null);
          setFetchedRegion(region);
        }
      })
      .catch((err: unknown) => {
        if (!cancelled) {
          setError(err instanceof Error ? err.message : 'Unknown error');
          setForecasts([]);
          setFetchedRegion(region);
        }
      });
    return () => {
      cancelled = true;
    };
  }, [region]);

  return (
    <div className="app">
      <header className="app-header">
        <h1>🌦️ DarkFactory Weather</h1>
        <p className="subtitle">5-day forecast by climate region</p>
      </header>

      <main className="app-main">
        <RegionSelector selected={region} onChange={setRegion} disabled={loading} />

        {loading && <p className="status-msg">Loading forecast…</p>}
        {!loading && error && <p className="status-msg error">⚠️ {error}</p>}
        {!loading && !error && <ForecastTable forecasts={forecasts} />}
      </main>
    </div>
  );
}

export default App;
