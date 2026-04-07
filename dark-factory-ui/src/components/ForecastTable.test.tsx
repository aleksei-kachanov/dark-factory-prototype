import { render, screen } from '@testing-library/react';
import { ForecastTable } from './ForecastTable';
import type { WeatherForecastDto } from '../types/weather';

const mockForecast: WeatherForecastDto = {
  date: '2026-04-08',
  temperatureC: 25,
  temperatureF: 77,
  summary: 'Warm',
  humidity: 60,
  windSpeed: 15.5,
  windDirection: 'NE',
};

describe('ForecastTable', () => {
  it('renders wind direction column header', () => {
    render(<ForecastTable forecasts={[mockForecast]} />);
    expect(screen.getByText('Wind Dir')).toBeInTheDocument();
  });

  it('renders wind direction value for each row', () => {
    render(<ForecastTable forecasts={[mockForecast]} />);
    expect(screen.getByText('NE')).toBeInTheDocument();
  });

  it('renders dash fallback when windDirection is empty string', () => {
    const forecastNoDir: WeatherForecastDto = { ...mockForecast, windDirection: '' };
    render(<ForecastTable forecasts={[forecastNoDir]} />);
    expect(screen.getByText('—')).toBeInTheDocument();
  });
});
