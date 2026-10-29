import { Link } from 'react-router-dom'

export function NotFoundPage() {
  return (
    <div className="d-flex flex-column align-items-center justify-content-center vh-100 text-center">
      <h1 className="display-4 fw-bold text-primary">404</h1>
      <p className="fs-5 text-muted mb-4">The page you are looking for does not exist.</p>
      <Link to="/" className="btn btn-primary">
        Back to Dashboard
      </Link>
    </div>
  )
}
