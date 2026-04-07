export interface WeatherForecastDto {
  date: string
  temperatureC: number
  temperatureF: number
  summary: string | null
  humidity: number
  windSpeed: number
  windDirection: string
}

export type Region = 'tropical' | 'arid' | 'temperate' | 'continental' | 'polar'
export const REGIONS: Region[] = ['tropical', 'arid', 'temperate', 'continental', 'polar']

export type TexasCitySlug = 'austin' | 'dallas' | 'houston' | 'san-antonio' | 'fort-worth'
export interface TexasCityMeta {
  slug: TexasCitySlug
  name: string
}
export const TEXAS_CITIES: TexasCityMeta[] = [
  { slug: 'austin',       name: 'Austin' },
  { slug: 'dallas',       name: 'Dallas' },
  { slug: 'houston',      name: 'Houston' },
  { slug: 'san-antonio',  name: 'San Antonio' },
  { slug: 'fort-worth',   name: 'Fort Worth' },
]

export type Selection = Region | TexasCitySlug

export const TEXAS_CITY_SLUGS: readonly TexasCitySlug[] =
  TEXAS_CITIES.map(c => c.slug) as TexasCitySlug[]

export function isTexasCity(s: Selection): s is TexasCitySlug {
  return TEXAS_CITY_SLUGS.includes(s as TexasCitySlug)
}
