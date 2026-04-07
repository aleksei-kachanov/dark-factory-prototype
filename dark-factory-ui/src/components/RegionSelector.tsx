import { REGIONS, TEXAS_CITIES, Selection } from '../types/weather'
import './RegionSelector.css'

interface Props {
  selected: Selection
  onChange: (value: Selection) => void
}

export function RegionSelector({ selected, onChange }: Props) {
  return (
    <select
      value={selected}
      onChange={e => onChange(e.target.value as Selection)}
      aria-label="Select region or city"
    >
      <optgroup label="Climate Regions">
        {REGIONS.map(r => (
          <option key={r} value={r}>
            {r.charAt(0).toUpperCase() + r.slice(1)}
          </option>
        ))}
      </optgroup>
      <optgroup label="Texas Cities">
        {TEXAS_CITIES.map(c => (
          <option key={c.slug} value={c.slug}>
            {c.name}
          </option>
        ))}
      </optgroup>
    </select>
  )
}
