import { Outlet } from 'react-router-dom'
import { Header } from '../components/Header'
import { Sidebar } from '../components/Sidebar'

export function DashboardLayout() {
  return (
    <div className="d-flex vh-100">
      <div className="d-none d-md-block">
        <Sidebar />
      </div>

      <div
        className="offcanvas offcanvas-start d-md-none"
        tabIndex={-1}
        id="sidebarOffcanvas"
        aria-labelledby="sidebarOffcanvasLabel"
      >
        <div className="offcanvas-header">
          <h5 className="offcanvas-title" id="sidebarOffcanvasLabel">
            BudgetFlow
          </h5>
          <button type="button" className="btn-close" data-bs-dismiss="offcanvas" aria-label="Close"></button>
        </div>
        <div className="offcanvas-body p-0">
          <Sidebar id="sidebar-mobile" />
        </div>
      </div>

      <div className="d-flex flex-column flex-grow-1 overflow-hidden">
        <Header />
        <main className="flex-grow-1 overflow-auto p-3 p-md-4 bg-body-secondary">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
