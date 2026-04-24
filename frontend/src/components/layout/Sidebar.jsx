import { NavLink, useNavigate } from 'react-router-dom'
import {
  LayoutDashboard, Receipt, Wallet, TrendingUp, FileText,
  Tag, RefreshCw, Bell, User, LogOut, Shield,
  PanelLeftClose, PanelLeftOpen
} from 'lucide-react'
import { clsx } from 'clsx'
import { useTranslation } from 'react-i18next'
import { useAuthStore } from '../../store/authStore'
import { useUiStore } from '../../store/uiStore'
import { authApi } from '../../api/auth.api'
import toast from 'react-hot-toast'

export default function Sidebar() {
  const { refreshToken, logout, isAdmin } = useAuthStore()
  const { sidebarCollapsed, toggleCollapsed } = useUiStore()
  const navigate = useNavigate()
  const { t, i18n } = useTranslation()
  const isRTL = i18n.language === 'ar'

  const navItems = [
    { to: '/dashboard', icon: LayoutDashboard, label: t('nav.dashboard') },
    { to: '/expenses',  icon: Receipt,         label: t('nav.expenses')  },
    { to: '/budgets',   icon: Wallet,          label: t('nav.budgets')   },
    { to: '/insights',  icon: TrendingUp,      label: t('nav.insights')  },
    { to: '/reports',   icon: FileText,        label: t('nav.reports')   },
  ]

  const otherItems = [
    { to: '/categories', icon: Tag,       label: t('nav.categories') },
    { to: '/recurring',  icon: RefreshCw, label: t('nav.recurring')  },
    { to: '/alerts',     icon: Bell,      label: t('nav.alerts')     },
  ]

  const handleLogout = async () => {
    try { await authApi.logout(refreshToken) } catch {}
    logout()
    navigate('/login')
    toast.success(t('nav.logout'))
  }

  const CollapseIcon = sidebarCollapsed
    ? (isRTL ? PanelLeftClose : PanelLeftOpen)
    : (isRTL ? PanelLeftOpen  : PanelLeftClose)

  return (
    <aside className={clsx(
      'hidden lg:flex flex-col bg-white border-e border-gray-100 transition-all duration-300 flex-shrink-0',
      sidebarCollapsed ? 'w-[68px]' : 'w-60'
    )}>
      {/* Logo + collapse button */}
      <div className={clsx(
        'flex items-center border-b border-gray-100 h-14 px-3 gap-2',
        sidebarCollapsed ? 'justify-center' : 'justify-between'
      )}>
        {/* Logo */}
        <div className={clsx('flex items-center gap-2.5 min-w-0', sidebarCollapsed && 'justify-center')}>
          <div className="w-8 h-8 bg-primary-400 rounded-lg flex items-center justify-center flex-shrink-0">
            <span className="text-white text-sm font-bold">ET</span>
          </div>
          {!sidebarCollapsed && (
            <span className="text-sm font-semibold text-gray-900 truncate">Expense Tracker</span>
          )}
        </div>

        {/* Collapse toggle — inline, professional */}
        {!sidebarCollapsed && (
          <button
            onClick={toggleCollapsed}
            title="Collapse sidebar"
            className="p-1.5 rounded-lg text-gray-400 hover:text-gray-700 hover:bg-gray-100 transition-colors flex-shrink-0"
          >
            <CollapseIcon size={17} />
          </button>
        )}
      </div>

      {/* Expand button when collapsed */}
      {sidebarCollapsed && (
        <button
          onClick={toggleCollapsed}
          title="Expand sidebar"
          className="mx-auto mt-2 p-1.5 rounded-lg text-gray-400 hover:text-gray-700 hover:bg-gray-100 transition-colors"
        >
          <CollapseIcon size={17} />
        </button>
      )}

      {/* Nav */}
      <nav className="flex-1 px-2 py-3 space-y-0.5 overflow-y-auto scrollbar-hide">
        {!sidebarCollapsed && (
          <p className="text-[11px] font-semibold text-gray-400 px-3 mb-1.5 uppercase tracking-wider">
            {t('nav.dashboard')}
          </p>
        )}
        {navItems.map(item => (
          <SidebarItem key={item.to} {...item} collapsed={sidebarCollapsed} />
        ))}

        <div className="my-2 border-t border-gray-100 mx-2" />

        {!sidebarCollapsed && (
          <p className="text-[11px] font-semibold text-gray-400 px-3 mb-1.5 uppercase tracking-wider">
            Other
          </p>
        )}
        {otherItems.map(item => (
          <SidebarItem key={item.to} {...item} collapsed={sidebarCollapsed} />
        ))}

        {isAdmin() && (
          <>
            <div className="my-2 border-t border-gray-100 mx-2" />
            {!sidebarCollapsed && (
              <p className="text-[11px] font-semibold text-gray-400 px-3 mb-1.5 uppercase tracking-wider">
                Admin
              </p>
            )}
            <SidebarItem to="/admin" icon={Shield} label={t('nav.admin')} collapsed={sidebarCollapsed} />
          </>
        )}
      </nav>

      {/* Footer */}
      <div className="border-t border-gray-100 px-2 py-2 space-y-0.5">
        <SidebarItem to="/profile" icon={User} label={t('nav.profile')} collapsed={sidebarCollapsed} />
        <button
          onClick={handleLogout}
          title={sidebarCollapsed ? t('nav.logout') : ''}
          className={clsx(
            'w-full flex items-center gap-3 px-3 py-2 rounded-lg text-sm',
            'text-gray-500 hover:bg-red-50 hover:text-red-500 transition-colors',
            sidebarCollapsed && 'justify-center'
          )}
        >
          <LogOut size={16} className="flex-shrink-0" />
          {!sidebarCollapsed && <span>{t('nav.logout')}</span>}
        </button>
      </div>
    </aside>
  )
}

function SidebarItem({ to, icon: Icon, label, collapsed }) {
  return (
    <NavLink
      to={to}
      title={collapsed ? label : ''}
      className={({ isActive }) => clsx(
        'flex items-center gap-3 px-3 py-2 rounded-lg text-sm transition-colors group',
        collapsed && 'justify-center',
        isActive
          ? 'bg-primary-50 text-primary-600 font-medium'
          : 'text-gray-500 hover:bg-gray-50 hover:text-gray-900'
      )}
    >
      <Icon size={17} className="flex-shrink-0" />
      {!collapsed && <span className="truncate">{label}</span>}
    </NavLink>
  )
}
