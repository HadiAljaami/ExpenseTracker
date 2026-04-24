import { NavLink, useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { X, LayoutDashboard, Receipt, Wallet, TrendingUp, FileText, Tag, RefreshCw, Bell, User, LogOut, Shield } from 'lucide-react'
import { clsx } from 'clsx'
import { useUiStore } from '../../store/uiStore'
import { useAuthStore } from '../../store/authStore'
import { authApi } from '../../api/auth.api'
import LangSwitcher from '../ui/LangSwitcher'
import toast from 'react-hot-toast'

export default function MobileSidebar() {
  const { sidebarOpen, setSidebarOpen } = useUiStore()
  const { refreshToken, logout, isAdmin } = useAuthStore()
  const { t } = useTranslation()
  const navigate = useNavigate()

  const allItems = [
    { to: '/dashboard', icon: LayoutDashboard, label: t('nav.dashboard') },
    { to: '/expenses', icon: Receipt, label: t('nav.expenses') },
    { to: '/budgets', icon: Wallet, label: t('nav.budgets') },
    { to: '/insights', icon: TrendingUp, label: t('nav.insights') },
    { to: '/reports', icon: FileText, label: t('nav.reports') },
    { to: '/categories', icon: Tag, label: t('nav.categories') },
    { to: '/recurring', icon: RefreshCw, label: t('nav.recurring') },
    { to: '/alerts', icon: Bell, label: t('nav.alerts') },
    { to: '/profile', icon: User, label: t('nav.profile') },
  ]

  const handleLogout = async () => {
    try { await authApi.logout(refreshToken) } catch {}
    logout()
    setSidebarOpen(false)
    navigate('/login')
    toast.success(t('nav.logout'))
  }

  if (!sidebarOpen) return null

  return (
    <div className="lg:hidden fixed inset-0 z-50 flex">
      <div className="absolute inset-0 bg-black/40" onClick={() => setSidebarOpen(false)} />
      <div className="relative w-64 bg-white h-full flex flex-col shadow-xl">
        <div className="flex items-center justify-between px-4 py-4 border-b border-gray-100">
          <div className="flex items-center gap-2">
            <div className="w-7 h-7 bg-primary-400 rounded-lg flex items-center justify-center">
              <span className="text-white text-xs font-bold">ET</span>
            </div>
            <span className="text-sm font-semibold">Expense Tracker</span>
          </div>
          <button onClick={() => setSidebarOpen(false)} className="p-1.5 text-gray-400 hover:bg-gray-100 rounded-lg"><X size={16} /></button>
        </div>
        <nav className="flex-1 px-3 py-3 space-y-0.5 overflow-y-auto">
          {allItems.map(item => (
            <NavLink key={item.to} to={item.to} onClick={() => setSidebarOpen(false)}
              className={({ isActive }) => clsx('flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm transition-colors',
                isActive ? 'bg-primary-50 text-primary-600 font-medium' : 'text-gray-600 hover:bg-gray-50')}>
              <item.icon size={17} />{item.label}
            </NavLink>
          ))}
          {isAdmin() && (
            <NavLink to="/admin" onClick={() => setSidebarOpen(false)}
              className={({ isActive }) => clsx('flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm transition-colors',
                isActive ? 'bg-primary-50 text-primary-600 font-medium' : 'text-gray-600 hover:bg-gray-50')}>
              <Shield size={17} />{t('nav.admin')}
            </NavLink>
          )}
        </nav>
        <div className="border-t border-gray-100 p-3 space-y-2">
          <LangSwitcher className="w-full justify-center" />
          <button onClick={handleLogout} className="w-full flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm text-red-500 hover:bg-red-50">
            <LogOut size={17} />{t('nav.logout')}
          </button>
        </div>
      </div>
    </div>
  )
}
