import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { RegionSelector } from './RegionSelector'
import { TEXAS_CITIES } from '../types/weather'

const noop = () => {}

describe('RegionSelector — Texas Cities', () => {
  it('renders Texas Cities optgroup', () => {
    render(<RegionSelector selected="tropical" onChange={noop} />)
    expect(screen.getByRole('group', { name: /texas cities/i })).toBeInTheDocument()
  })

  it('renders all 5 Texas cities in the Texas Cities group', () => {
    render(<RegionSelector selected="tropical" onChange={noop} />)
    const group = screen.getByRole('group', { name: /texas cities/i })
    const options = Array.from(group.querySelectorAll('option'))
    const slugs = options.map(o => (o as HTMLOptionElement).value)
    expect(slugs).toContain('austin')
    expect(slugs).toContain('dallas')
    expect(slugs).toContain('houston')
    expect(slugs).toContain('san-antonio')
    expect(slugs).toContain('fort-worth')
  })

  it('renders Climate Regions optgroup', () => {
    render(<RegionSelector selected="tropical" onChange={noop} />)
    expect(screen.getByRole('group', { name: /climate regions/i })).toBeInTheDocument()
  })

  it('Austin does NOT appear in Climate Regions group', () => {
    render(<RegionSelector selected="tropical" onChange={noop} />)
    const regionsGroup = screen.getByRole('group', { name: /climate regions/i })
    const options = Array.from(regionsGroup.querySelectorAll('option'))
    const slugs = options.map(o => (o as HTMLOptionElement).value)
    expect(slugs).not.toContain('austin')
  })

  it('calls onChange with texas city slug when user selects Dallas', async () => {
    const onChange = vi.fn()
    render(<RegionSelector selected="tropical" onChange={onChange} />)
    await userEvent.selectOptions(screen.getByRole('combobox'), 'dallas')
    expect(onChange).toHaveBeenCalledWith('dallas')
  })

  it('total options count equals 10 (5 regions + 5 texas cities)', () => {
    render(<RegionSelector selected="tropical" onChange={noop} />)
    const select = screen.getByRole('combobox')
    const options = Array.from(select.querySelectorAll('option'))
    expect(options).toHaveLength(10)
  })

  it('selected texas city is marked as selected', () => {
    render(<RegionSelector selected="houston" onChange={noop} />)
    const select = screen.getByRole('combobox') as HTMLSelectElement
    expect(select.value).toBe('houston')
  })
})
