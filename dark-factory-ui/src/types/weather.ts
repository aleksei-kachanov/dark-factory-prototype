export interface WeatherForecastDto {
  date: string;
  temperatureC: number;
  temperatureF: number;
  summary: string | null;
  humidity: number;
  windSpeed: number;
  windDirection: string;
}

export type Region = 'tropical' | 'arid' | 'temperate' | 'continental' | 'polar' | 'austin';

export const REGIONS: Region[] = ['tropical', 'arid', 'temperate', 'continental', 'polar', 'austin'];

export const REGION_LABELS: Record<Region, string> = {
  tropical: 'Tropical',
  arid: 'Arid',
  temperate: 'Temperate',
  continental: 'Continental',
  polar: 'Polar',
  austin: 'Austin, TX',
};
