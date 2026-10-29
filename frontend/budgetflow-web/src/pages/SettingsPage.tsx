import { useTheme } from '../hooks/useTheme'
import { useAuth } from '../hooks/useAuth'

export function SettingsPage() {
  const { theme, toggleTheme } = useTheme()
  const { user, logout } = useAuth()

  return (
    <div>
      <h1 className="h3 mb-4">Settings</h1>

      <div className="card mb-3">
        <div className="card-body">
          <h6 className="card-subtitle text-muted mb-3">Profile</h6>
          <p className="mb-1">
            <strong>Email:</strong> {user?.email ?? '—'}
          </p>
          <p className="mb-0">
            <strong>Name:</strong> {user?.displayName ?? '—'}
          </p>
        </div>
      </div>

      <div className="card mb-3">
        <div className="card-body">
          <h6 className="card-subtitle text-muted mb-3">Appearance</h6>
          <div className="form-check form-switch">
            <input
              className="form-check-input"
              type="checkbox"
              role="switch"
              id="darkModeSwitch"
              checked={theme === 'dark'}
              onChange={toggleTheme}
            />
            <label className="form-check-label" htmlFor="darkModeSwitch">
              Dark mode
            </label>
          </div>
        </div>
      </div>

      <div className="card">
        <div className="card-body">
          <h6 className="card-subtitle text-muted mb-3">Account</h6>
          <button type="button" className="btn btn-outline-danger" onClick={logout}>
            <i className="bi bi-box-arrow-right me-1" aria-hidden="true"></i>
            Sign out
          </button>
        </div>
      </div>
    </div>
  )
}
