export interface WeatherForecastDto {
  date: string;
  temperatureC: number;
  temperatureF: number;
  summary: string | null;
  humidity: number;
  windSpeed: number;
}

export type Region = 'tropical' | 'arid' | 'temperate' | 'continental' | 'polar';

export const REGIONS: Region[] = ['tropical', 'arid', 'temperate', 'continental', 'polar'];
