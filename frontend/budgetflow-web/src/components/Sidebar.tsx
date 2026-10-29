import { NavLink } from 'react-router-dom'
import type { NavItem } from '../types/nav'

const navItems: NavItem[] = [
  { label: 'Dashboard', path: '/', icon: 'bi-speedometer2' },
  { label: 'Accounts', path: '/accounts', icon: 'bi-bank' },
  { label: 'Transactions', path: '/transactions', icon: 'bi-receipt' },
  { label: 'Bills', path: '/bills', icon: 'bi-file-earmark-text' },
  { label: 'Budgets', path: '/budgets', icon: 'bi-wallet2' },
  { label: 'Savings Goals', path: '/savings-goals', icon: 'bi-piggy-bank' },
  { label: 'Reports', path: '/reports', icon: 'bi-bar-chart-line' },
  { label: 'Settings', path: '/settings', icon: 'bi-gear' },
]

interface SidebarProps {
  id?: string
}

export function Sidebar({ id = 'sidebar' }: SidebarProps) {
  return (
    <nav id={id} className="d-flex flex-column flex-shrink-0 p-3 bg-body-tertiary border-end h-100" style={{ width: '250px' }}>
      <span className="d-flex align-items-center mb-3 mb-md-0 me-md-auto fs-4 fw-semibold">
        <i className="bi bi-piggy-bank-fill me-2 text-primary" aria-hidden="true"></i>
        BudgetFlow
      </span>
      <hr />
      <ul className="nav nav-pills flex-column mb-auto">
        {navItems.map((item) => (
          <li className="nav-item" key={item.path}>
            <NavLink
              to={item.path}
              end={item.path === '/'}
              className={({ isActive }) => `nav-link d-flex align-items-center gap-2 ${isActive ? 'active' : 'link-body-emphasis'}`}
            >
              <i className={`bi ${item.icon}`} aria-hidden="true"></i>
              {item.label}
            </NavLink>
          </li>
        ))}
      </ul>
    </nav>
  )
}
