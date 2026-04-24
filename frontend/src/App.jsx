import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { Toaster } from 'react-hot-toast'
import { lazy, Suspense } from 'react'
import AppLayout from './components/layout/AppLayout'
import { ProtectedRoute, AdminRoute, GuestRoute } from './router/ProtectedRoute'
import { SkeletonCard } from './components/ui/Skeleton'

const LoginPage      = lazy(() => import('./pages/auth/LoginPage'))
const RegisterPage   = lazy(() => import('./pages/auth/RegisterPage'))
const ForgotPassword = lazy(() => import('./pages/auth/ForgotPasswordPage'))
const ResetPassword  = lazy(() => import('./pages/auth/ResetPasswordPage'))
const DashboardPage  = lazy(() => import('./pages/dashboard/DashboardPage'))
const ExpensesPage   = lazy(() => import('./pages/expenses/ExpensesPage'))
const BudgetsPage    = lazy(() => import('./pages/budgets/BudgetsPage'))
const InsightsPage   = lazy(() => import('./pages/insights/InsightsPage'))
const ReportsPage    = lazy(() => import('./pages/reports/ReportsPage'))
const CategoriesPage = lazy(() => import('./pages/categories/CategoriesPage'))
const RecurringPage  = lazy(() => import('./pages/recurring/RecurringPage'))
const AlertsPage     = lazy(() => import('./pages/alerts/AlertsPage'))
const ProfilePage    = lazy(() => import('./pages/profile/ProfilePage'))
const AdminPage      = lazy(() => import('./pages/admin/AdminPage'))

const queryClient = new QueryClient({
  defaultOptions: {
    queries: { retry: 1, staleTime: 1000 * 60 * 5, refetchOnWindowFocus: false },
  },
})

function PageLoader() {
  return (
    <div className="p-6 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      {[...Array(4)].map((_, i) => <SkeletonCard key={i} />)}
    </div>
  )
}

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Suspense fallback={<PageLoader />}>
          <Routes>
            <Route element={<GuestRoute />}>
              <Route path="/login" element={<LoginPage />} />
              <Route path="/register" element={<RegisterPage />} />
              <Route path="/forgot-password" element={<ForgotPassword />} />
              <Route path="/reset-password" element={<ResetPassword />} />
            </Route>
            <Route element={<ProtectedRoute />}>
              <Route element={<AppLayout />}>
                <Route path="/dashboard"  element={<DashboardPage />} />
                <Route path="/expenses"   element={<ExpensesPage />} />
                <Route path="/budgets"    element={<BudgetsPage />} />
                <Route path="/insights"   element={<InsightsPage />} />
                <Route path="/reports"    element={<ReportsPage />} />
                <Route path="/categories" element={<CategoriesPage />} />
                <Route path="/recurring"  element={<RecurringPage />} />
                <Route path="/alerts"     element={<AlertsPage />} />
                <Route path="/profile"    element={<ProfilePage />} />
                <Route element={<AdminRoute />}>
                  <Route path="/admin/*" element={<AdminPage />} />
                </Route>
              </Route>
            </Route>
            <Route path="/" element={<Navigate to="/dashboard" replace />} />
            <Route path="*" element={<Navigate to="/dashboard" replace />} />
          </Routes>
        </Suspense>
      </BrowserRouter>
      <Toaster position="top-right" toastOptions={{
        duration: 3000,
        style: { fontSize: '13px', borderRadius: '10px', border: '1px solid #f0f0f0' },
        success: { iconTheme: { primary: '#1D9E75', secondary: '#fff' } },
      }} />
    </QueryClientProvider>
  )
}
