import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import App from '../App'

const mockForecast = [
  { date: '2026-04-07', temperatureC: 28, temperatureF: 82, summary: 'Sunny', humidity: 55, windSpeed: 12, windDirection: 'S' }
]

describe('App — Texas city fetch routing', () => {
  beforeEach(() => {
    vi.stubGlobal('fetch', vi.fn())
  })

  it('fetches /weather/city/dallas when Dallas is selected', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => mockForecast
    })
    vi.stubGlobal('fetch', fetchMock)

    render(<App />)
    await userEvent.selectOptions(screen.getByRole('combobox'), 'dallas')

    await waitFor(() => {
      const calls = fetchMock.mock.calls.map((c: unknown[]) => c[0] as string)
      expect(calls.some(url => url.includes('/weather/city/dallas'))).toBe(true)
    })
  })

  it('fetches /weather/tropical for simulated region (no regression)', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => mockForecast
    })
    vi.stubGlobal('fetch', fetchMock)

    render(<App />)
    // Default or explicit tropical selection
    await userEvent.selectOptions(screen.getByRole('combobox'), 'tropical')

    await waitFor(() => {
      const calls = fetchMock.mock.calls.map((c: unknown[]) => c[0] as string)
      expect(calls.some(url => url.includes('/weather/tropical'))).toBe(true)
    })
  })

  it('shows error UI when Texas city fetch fails', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 502 }))

    render(<App />)
    await userEvent.selectOptions(screen.getByRole('combobox'), 'houston')

    await waitFor(() => {
      expect(screen.queryByText(/error|failed|unavailable/i)).toBeTruthy()
    })
  })
})
