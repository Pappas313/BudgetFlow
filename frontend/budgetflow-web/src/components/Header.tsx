import { useAuth } from '../hooks/useAuth'
import { useTheme } from '../hooks/useTheme'

export function Header() {
  const { user, isAuthenticated, logout } = useAuth()
  const { theme, toggleTheme } = useTheme()

  return (
    <header className="navbar navbar-expand bg-body border-bottom px-3 py-2">
      <button
        className="btn btn-outline-secondary d-md-none me-2"
        type="button"
        data-bs-toggle="offcanvas"
        data-bs-target="#sidebarOffcanvas"
        aria-controls="sidebarOffcanvas"
        aria-label="Toggle navigation"
      >
        <i className="bi bi-list" aria-hidden="true"></i>
      </button>

      <span className="navbar-text fw-semibold d-none d-sm-inline">Welcome back{user ? `, ${user.displayName}` : ''}</span>

      <div className="ms-auto d-flex align-items-center gap-2">
        <button
          type="button"
          className="btn btn-outline-secondary btn-sm"
          onClick={toggleTheme}
          aria-label="Toggle theme"
        >
          <i className={`bi ${theme === 'dark' ? 'bi-sun-fill' : 'bi-moon-stars-fill'}`} aria-hidden="true"></i>
        </button>

        {isAuthenticated && (
          <button type="button" className="btn btn-outline-danger btn-sm" onClick={logout}>
            <i className="bi bi-box-arrow-right me-1" aria-hidden="true"></i>
            Sign out
          </button>
        )}
      </div>
    </header>
  )
}
