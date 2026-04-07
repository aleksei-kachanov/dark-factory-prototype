import { render, screen } from '@testing-library/react';
import { RegionSelector } from './RegionSelector';

const noop = () => {};

describe('RegionSelector', () => {
  it('renders Austin TX label for austin region', () => {
    render(<RegionSelector selected="austin" onChange={noop} />);
    expect(screen.getByText('Austin, TX')).toBeInTheDocument();
  });

  it('renders all six regions', () => {
    render(<RegionSelector selected="tropical" onChange={noop} />);
    const buttons = screen.getAllByRole('button');
    expect(buttons).toHaveLength(6);
  });

  it('renders correct label for each region', () => {
    render(<RegionSelector selected="tropical" onChange={noop} />);
    expect(screen.getByText('Tropical')).toBeInTheDocument();
    expect(screen.getByText('Arid')).toBeInTheDocument();
    expect(screen.getByText('Temperate')).toBeInTheDocument();
    expect(screen.getByText('Continental')).toBeInTheDocument();
    expect(screen.getByText('Polar')).toBeInTheDocument();
    expect(screen.getByText('Austin, TX')).toBeInTheDocument();
  });
});
