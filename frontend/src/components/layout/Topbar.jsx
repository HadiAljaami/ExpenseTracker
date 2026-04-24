import { useState } from 'react'
import { Menu, Bell, X } from 'lucide-react'
import { useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { useUiStore } from '../../store/uiStore'
import { useAuthStore } from '../../store/authStore'
import { alertsApi } from '../../api/index'
import { QUERY_KEYS } from '../../constants/queryKeys'
import { formatRelativeTime } from '../../utils/formatters'
import LangSwitcher from '../ui/LangSwitcher'

export default function Topbar({ title, actions }) {
  const { setSidebarOpen } = useUiStore()
  const { user } = useAuthStore()
  const [alertsOpen, setAlertsOpen] = useState(false)
  const navigate = useNavigate()
  const { t } = useTranslation()

  const { data: alertsData } = useQuery({
    queryKey: QUERY_KEYS.ALERTS,
    queryFn: () => alertsApi.getAll({ unreadOnly: true, pageSize: 5 }),
    select: (res) => res.data.data,
    refetchInterval: 30000,
  })

  const unreadCount = alertsData?.unreadCount || 0

  return (
    <header className="bg-white border-b border-gray-100 flex-shrink-0">
      <div className="flex items-center justify-between px-4 lg:px-6 h-14 gap-3">
        {/* Left */}
        <div className="flex items-center gap-3 min-w-0">
          <button onClick={() => setSidebarOpen(true)}
            className="lg:hidden p-2 -ml-1 text-gray-500 hover:bg-gray-100 rounded-lg flex-shrink-0">
            <Menu size={18} />
          </button>
          <h1 className="text-sm sm:text-base font-semibold text-gray-900 truncate">{title}</h1>
        </div>

        {/* Right */}
        <div className="flex items-center gap-2 flex-shrink-0">
          {/* Actions desktop */}
          {actions && <div className="hidden sm:flex items-center gap-2">{actions}</div>}

          {/* Language switcher */}
          <LangSwitcher className="hidden sm:flex" />

          {/* Bell */}
          <div className="relative">
            <button onClick={() => setAlertsOpen(!alertsOpen)}
              className="relative p-2 text-gray-500 hover:bg-gray-100 rounded-lg transition-colors">
              <Bell size={18} />
              {unreadCount > 0 && (
                <span className="absolute top-1.5 right-1.5 w-3.5 h-3.5 bg-danger-400 text-white text-[10px] rounded-full flex items-center justify-center font-medium">
                  {unreadCount > 9 ? '9+' : unreadCount}
                </span>
              )}
            </button>

            {alertsOpen && (
              <>
                <div className="fixed inset-0 z-20" onClick={() => setAlertsOpen(false)} />
                <div className="absolute end-0 top-11 w-72 sm:w-80 bg-white rounded-xl shadow-dropdown border border-gray-100 z-30 overflow-hidden">
                  <div className="flex items-center justify-between px-4 py-3 border-b border-gray-100">
                    <span className="text-sm font-semibold text-gray-900">{t('nav.alerts')}</span>
                    <button onClick={() => setAlertsOpen(false)} className="text-gray-400 hover:text-gray-600 p-1"><X size={14} /></button>
                  </div>
                  <div className="max-h-64 overflow-y-auto">
                    {!alertsData?.items?.length ? (
                      <p className="text-sm text-gray-400 text-center py-8">{t('alerts.noAlerts')}</p>
                    ) : alertsData.items.map((alert) => (
                      <div key={alert.id} className="px-4 py-3 border-b border-gray-50 hover:bg-gray-50">
                        <p className="text-xs font-semibold text-gray-900">{alert.title}</p>
                        <p className="text-xs text-gray-500 mt-0.5 line-clamp-2">{alert.message}</p>
                        <p className="text-xs text-gray-400 mt-1">{formatRelativeTime(alert.createdAt)}</p>
                      </div>
                    ))}
                  </div>
                  <button onClick={() => { setAlertsOpen(false); navigate('/alerts') }}
                    className="w-full py-3 text-xs font-medium text-primary-600 hover:bg-primary-50 transition-colors">
                    {t('common.view')} {t('nav.alerts')}
                  </button>
                </div>
              </>
            )}
          </div>

          {/* Avatar */}
          <button onClick={() => navigate('/profile')}
            className="w-8 h-8 rounded-full bg-primary-50 flex items-center justify-center text-primary-600 text-xs font-semibold hover:bg-primary-100 transition-colors flex-shrink-0">
            {user?.fullName?.slice(0, 2).toUpperCase() || 'U'}
          </button>
        </div>
      </div>

      {/* Mobile actions + lang switcher */}
      {(actions) && (
        <div className="sm:hidden flex items-center gap-2 px-4 pb-3 pt-2 border-t border-gray-50">
          {actions}
          <LangSwitcher />
        </div>
      )}
    </header>
  )
}
