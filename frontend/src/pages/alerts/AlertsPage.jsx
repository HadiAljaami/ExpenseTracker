import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { Bell, Check, CheckCheck } from 'lucide-react'
import PageLayout from '../../components/layout/PageLayout'
import Card from '../../components/ui/Card'
import Button from '../../components/ui/Button'
import EmptyState from '../../components/ui/EmptyState'
import Badge from '../../components/ui/Badge'
import { alertsApi } from '../../api/index'
import { QUERY_KEYS } from '../../constants/queryKeys'
import { formatRelativeTime } from '../../utils/formatters'
import { clsx } from 'clsx'
import toast from 'react-hot-toast'

const TYPE_STYLES = {
  BudgetExceeded: 'red',
  BudgetWarning: 'amber',
  SpendingSpike: 'blue',
}

export default function AlertsPage() {
  const { t } = useTranslation()
  const qc = useQueryClient()

  const { data, isLoading } = useQuery({
    queryKey: QUERY_KEYS.ALERTS,
    queryFn: () => alertsApi.getAll({ pageSize: 50 }),
    select: res => res.data.data,
  })

  const markOneMutation = useMutation({
    mutationFn: alertsApi.markAsRead,
    onSuccess: () => qc.invalidateQueries({ queryKey: QUERY_KEYS.ALERTS }),
  })
  const markAllMutation = useMutation({
    mutationFn: alertsApi.markAllAsRead,
    onSuccess: () => { qc.invalidateQueries({ queryKey: QUERY_KEYS.ALERTS }); toast.success(t('alerts.allMarked')) },
  })

  const alerts = data?.items || []
  const unreadCount = data?.unreadCount || 0

  const getTypeLabel = (type) => {
    const map = { BudgetExceeded: t('alerts.budgetExceeded'), BudgetWarning: t('alerts.budgetWarning'), SpendingSpike: t('alerts.spendingSpike') }
    return map[type] || type
  }

  return (
    <PageLayout title={t('alerts.title')}
      actions={unreadCount > 0 && (
        <Button variant="secondary" size="sm" icon={CheckCheck} loading={markAllMutation.isPending}
          onClick={() => markAllMutation.mutate()}>
          {t('alerts.markAllRead', { count: unreadCount })}
        </Button>
      )}>
      {isLoading ? (
        <div className="space-y-3">{[...Array(4)].map((_, i) => <div key={i} className="skeleton h-16 rounded-lg" />)}</div>
      ) : alerts.length === 0 ? (
        <Card><EmptyState icon={Bell} title={t('alerts.noAlerts')} description={t('alerts.noAlertsDesc')} /></Card>
      ) : (
        <div className="space-y-2">
          {alerts.map(alert => (
            <div key={alert.id} className={clsx('card p-4 flex items-start gap-3 transition-colors',
              !alert.isRead && 'border-s-2 border-primary-400 bg-primary-50/30')}>
              <div className={clsx('w-9 h-9 rounded-full flex items-center justify-center flex-shrink-0',
                !alert.isRead ? 'bg-primary-100' : 'bg-gray-100')}>
                <Bell size={16} className={!alert.isRead ? 'text-primary-600' : 'text-gray-400'} />
              </div>
              <div className="flex-1 min-w-0">
                <div className="flex items-start justify-between gap-2">
                  <div className="min-w-0">
                    <p className={clsx('text-sm font-semibold', !alert.isRead ? 'text-gray-900' : 'text-gray-700')}>{alert.title}</p>
                    <p className="text-xs text-gray-500 mt-0.5 line-clamp-2">{alert.message}</p>
                  </div>
                  <Badge variant={TYPE_STYLES[alert.type] || 'gray'} className="flex-shrink-0 hidden sm:inline-flex">
                    {getTypeLabel(alert.type)}
                  </Badge>
                </div>
                <div className="flex items-center justify-between mt-2">
                  <span className="text-xs text-gray-400">{formatRelativeTime(alert.createdAt)}</span>
                  {!alert.isRead && (
                    <button onClick={() => markOneMutation.mutate(alert.id)}
                      className="flex items-center gap-1 text-xs text-primary-600 hover:underline">
                      <Check size={12} /> {t('alerts.markRead')}
                    </button>
                  )}
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </PageLayout>
  )
}
