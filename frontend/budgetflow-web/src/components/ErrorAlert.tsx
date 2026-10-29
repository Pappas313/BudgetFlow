interface ErrorAlertProps {
  message: string
  onRetry?: () => void
}

export function ErrorAlert({ message, onRetry }: ErrorAlertProps) {
  return (
    <div className="alert alert-danger d-flex align-items-center justify-content-between" role="alert">
      <span>
        <i className="bi bi-exclamation-triangle-fill me-2" aria-hidden="true"></i>
        {message}
      </span>
      {onRetry && (
        <button type="button" className="btn btn-sm btn-outline-danger" onClick={onRetry}>
          Retry
        </button>
      )}
    </div>
  )
}
