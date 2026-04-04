import type { Region } from '../types/weather';
import { REGIONS } from '../types/weather';
import './RegionSelector.css';

interface RegionSelectorProps {
  selected: Region;
  onChange: (region: Region) => void;
  disabled?: boolean;
}

export function RegionSelector({ selected, onChange, disabled }: RegionSelectorProps) {
  return (
    <div className="region-selector">
      {REGIONS.map((region) => (
        <button
          key={region}
          className={`region-btn${selected === region ? ' active' : ''}`}
          onClick={() => onChange(region)}
          disabled={disabled}
        >
          {region.charAt(0).toUpperCase() + region.slice(1)}
        </button>
      ))}
    </div>
  );
}
