import type { WeatherForecastDto } from '../types/weather';
import './ForecastTable.css';

interface ForecastTableProps {
  forecasts: WeatherForecastDto[];
}

const weatherEmoji: Record<string, string> = {
  Freezing: '🥶',
  Bracing: '🌬️',
  Chilly: '🌥️',
  Cool: '🌤️',
  Mild: '🌤️',
  Warm: '☀️',
  Balmy: '🌞',
  Hot: '🔥',
  Sweltering: '🌡️',
  Scorching: '🌵',
};

function getEmoji(summary: string | null): string {
  if (!summary) return '🌡️';
  return weatherEmoji[summary] ?? '🌡️';
}

function formatDate(dateStr: string): string {
  const [y, m, d] = dateStr.split('-').map(Number);
  return new Date(y, m - 1, d).toLocaleDateString(undefined, { weekday: 'short', month: 'short', day: 'numeric' });
}

export function ForecastTable({ forecasts }: ForecastTableProps) {
  if (forecasts.length === 0) return null;

  return (
    <div className="forecast-table-wrapper">
      <table className="forecast-table">
        <thead>
          <tr>
            <th>Date</th>
            <th>Condition</th>
            <th>Temp (°C)</th>
            <th>Temp (°F)</th>
            <th>Humidity (%)</th>
            <th>Wind (km/h)</th>
            <th>Wind Dir</th>
          </tr>
        </thead>
        <tbody>
          {forecasts.map((f) => (
            <tr key={f.date}>
              <td>{formatDate(f.date)}</td>
              <td className="summary-cell">
                <span className="emoji">{getEmoji(f.summary)}</span>
                {f.summary ?? '—'}
              </td>
              <td>{f.temperatureC}°</td>
              <td>{f.temperatureF}°</td>
              <td>{f.humidity}%</td>
              <td>{f.windSpeed.toFixed(1)}</td>
              <td>{f.windDirection || '—'}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
