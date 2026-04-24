import { useQuery } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { Wallet, TrendingUp, TrendingDown, Receipt, Plus, Download, ArrowRight, AlertCircle } from 'lucide-react'
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, PieChart, Pie, Cell } from 'recharts'
import PageLayout from '../../components/layout/PageLayout'
import StatCard from '../../components/ui/StatCard'
import Card, { CardHeader } from '../../components/ui/Card'
import Button from '../../components/ui/Button'
import Badge from '../../components/ui/Badge'
import ChartWrapper from '../../components/ui/ChartWrapper'
import { SkeletonCard } from '../../components/ui/Skeleton'
import { insightsApi, alertsApi } from '../../api/index'
import { QUERY_KEYS } from '../../constants/queryKeys'
import { formatCurrency, formatDateShort, getCurrentMonthYear } from '../../utils/formatters'
import { useAuthStore } from '../../store/authStore'

const COLORS = ['#1D9E75','#378ADD','#BA7517','#E24B4A','#7F77DD','#DDA0DD','#FF6B6B','#B0BEC5']

export default function DashboardPage() {
  const { user } = useAuthStore()
  const navigate = useNavigate()
  const { t } = useTranslation()
  const { month, year } = getCurrentMonthYear()

  const { data: insights, isLoading } = useQuery({
    queryKey: QUERY_KEYS.INSIGHTS(month, year),
    queryFn: () => insightsApi.getSummary({ month, year }),
    select: res => res.data.data,
  })

  const { data: alertsData } = useQuery({
    queryKey: QUERY_KEYS.ALERTS,
    queryFn: () => alertsApi.getAll({ unreadOnly: true, pageSize: 3 }),
    select: res => res.data.data,
  })

  const unreadAlerts = alertsData?.items || []

  if (isLoading) return (
    <PageLayout title={t('dashboard.title')}>
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
        {[...Array(4)].map((_, i) => <SkeletonCard key={i} />)}
      </div>
    </PageLayout>
  )

  const dailyData = insights?.dailyTrend?.slice(-14).map(d => ({
    date: formatDateShort(d.date), amount: d.amount
  })) || []

  const pieData = insights?.categoryBreakdown?.slice(0, 6).map(c => ({
    name: c.categoryName, value: c.totalAmount, icon: c.categoryIcon
  })) || []

  return (
    <PageLayout
      title={t('dashboard.greeting', { name: user?.fullName?.split(' ')[0] })}
      actions={
        <div className="flex items-center gap-2">
          <Button variant="secondary" size="sm" icon={Download} onClick={() => navigate('/reports')}>
            {t('common.export')}
          </Button>
          <Button variant="primary" size="sm" icon={Plus} onClick={() => navigate('/expenses')}>
            {t('dashboard.addExpense')}
          </Button>
        </div>
      }
    >
      {/* Alerts banner */}
      {unreadAlerts.length > 0 && (
        <div className="mb-5 space-y-2">
          {unreadAlerts.map(alert => (
            <div key={alert.id} className="flex items-start gap-3 p-3.5 bg-amber-50 border border-amber-200 rounded-lg">
              <AlertCircle size={16} className="text-warning-400 flex-shrink-0 mt-0.5" />
              <div className="flex-1 min-w-0">
                <p className="text-sm font-medium text-gray-800">{alert.title}</p>
                <p className="text-xs text-gray-500 mt-0.5 line-clamp-1">{alert.message}</p>
              </div>
              <button onClick={() => navigate('/alerts')} className="text-xs text-warning-400 hover:underline flex-shrink-0">
                {t('common.view')}
              </button>
            </div>
          ))}
        </div>
      )}

      {/* Stats */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-5">
        <StatCard
          label={t('dashboard.thisMonth')}
          value={formatCurrency(insights?.totalThisMonth || 0, user?.currency)}
          change={insights?.changePercentage}
          changeLabel={t('dashboard.vsLastMonth')}
          icon={Wallet} iconColor="text-primary-400" iconBg="bg-primary-50"
        />
        <StatCard
          label={t('dashboard.dailyAverage')}
          value={formatCurrency(insights?.averageDailySpending || 0, user?.currency)}
          icon={TrendingUp} iconColor="text-info-400" iconBg="bg-info-50"
        />
        <StatCard
          label={t('dashboard.topCategory')}
          value={insights?.highestSpendingCategory || '—'}
          changeLabel={formatCurrency(insights?.highestCategoryAmount || 0, user?.currency)}
          icon={Receipt} iconColor="text-warning-400" iconBg="bg-warning-50"
        />
        <StatCard
          label={t('dashboard.lastMonth')}
          value={formatCurrency(insights?.totalLastMonth || 0, user?.currency)}
          icon={TrendingDown} iconColor="text-gray-400" iconBg="bg-gray-100"
        />
      </div>

      {/* Charts row */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4 mb-4">
        {/* Bar chart */}
        <Card className="lg:col-span-2">
          <CardHeader
            title={t('dashboard.dailySpending')}
            action={
              <button onClick={() => navigate('/insights')}
                className="text-xs text-primary-600 hover:underline flex items-center gap-1">
                {t('dashboard.fullInsights')} <ArrowRight size={12} />
              </button>
            }
          />
          {dailyData.length > 0 ? (
            <ChartWrapper height={200}>
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={dailyData} margin={{ top: 4, right: 4, left: -20, bottom: 20 }}>
                  <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" vertical={false} />
                  <XAxis dataKey="date" tick={{ fontSize: 11, fill: '#9CA3AF' }} axisLine={false} tickLine={false} />
                  <YAxis tick={{ fontSize: 11, fill: '#9CA3AF' }} axisLine={false} tickLine={false} />
                  <Tooltip
                    cursor={{ fill: '#f9fafb' }}
                    contentStyle={{ fontSize: 12, borderRadius: 8, border: '1px solid #e5e7eb', boxShadow: '0 4px 6px -1px rgba(0,0,0,0.07)' }}
                    formatter={v => [formatCurrency(v, user?.currency), '']}
                  />
                  <Bar dataKey="amount" fill="#1D9E75" radius={[4, 4, 0, 0]} maxBarSize={28} />
                </BarChart>
              </ResponsiveContainer>
            </ChartWrapper>
          ) : (
            <div className="h-48 flex items-center justify-center text-sm text-gray-400">{t('common.noData')}</div>
          )}
        </Card>

        {/* Pie chart */}
        <Card>
          <CardHeader title={t('dashboard.categoryBreakdown')} />
          {pieData.length > 0 ? (
            <>
              <ChartWrapper height={150}>
                <ResponsiveContainer width="100%" height="100%">
                  <PieChart>
                    <Pie data={pieData} cx="50%" cy="50%" innerRadius={42} outerRadius={65}
                      dataKey="value" paddingAngle={2} strokeWidth={0}>
                      {pieData.map((_, i) => <Cell key={i} fill={COLORS[i % COLORS.length]} />)}
                    </Pie>
                    <Tooltip
                      contentStyle={{ fontSize: 12, borderRadius: 8, border: '1px solid #e5e7eb' }}
                      formatter={v => [formatCurrency(v, user?.currency), '']}
                    />
                  </PieChart>
                </ResponsiveContainer>
              </ChartWrapper>
              <div className="space-y-2 mt-3">
                {pieData.slice(0, 4).map((item, i) => (
                  <div key={i} className="flex items-center gap-2">
                    <div className="w-2.5 h-2.5 rounded-full flex-shrink-0" style={{ background: COLORS[i % COLORS.length] }} />
                    <span className="text-xs text-gray-600 flex-1 truncate">{item.icon} {item.name}</span>
                    <span className="text-xs font-semibold text-gray-800">{formatCurrency(item.value, user?.currency)}</span>
                  </div>
                ))}
              </div>
            </>
          ) : (
            <div className="h-48 flex items-center justify-center text-sm text-gray-400">{t('common.noData')}</div>
          )}
        </Card>
      </div>

      {/* Top categories */}
      <Card>
        <CardHeader
          title={t('dashboard.topCategories')}
          action={
            <button onClick={() => navigate('/insights')}
              className="text-xs text-primary-600 hover:underline flex items-center gap-1">
              {t('dashboard.viewAll')} <ArrowRight size={12} />
            </button>
          }
        />
        {insights?.categoryBreakdown?.length > 0 ? (
          <div className="space-y-3">
            {insights.categoryBreakdown.slice(0, 5).map((cat, i) => (
              <div key={i} className="flex items-center gap-3">
                <div className="w-9 h-9 rounded-xl bg-gray-50 flex items-center justify-center text-lg flex-shrink-0">
                  {cat.categoryIcon}
                </div>
                <div className="flex-1 min-w-0">
                  <div className="flex items-center justify-between mb-1.5">
                    <span className="text-sm font-medium text-gray-800 truncate">{cat.categoryName}</span>
                    <div className="flex items-center gap-3 flex-shrink-0 ms-2">
                      <span className="text-xs text-gray-400">{cat.transactionCount} {t('dashboard.transactions')}</span>
                      <span className="text-sm font-semibold text-gray-900">{formatCurrency(cat.totalAmount, user?.currency)}</span>
                    </div>
                  </div>
                  <div className="h-1.5 bg-gray-100 rounded-full overflow-hidden">
                    <div className="h-full rounded-full transition-all duration-700"
                      style={{ width: `${cat.percentage}%`, background: COLORS[i % COLORS.length] }} />
                  </div>
                </div>
                <Badge variant="gray" className="flex-shrink-0 w-12 justify-center">
                  {cat.percentage.toFixed(1)}%
                </Badge>
              </div>
            ))}
          </div>
        ) : (
          <div className="py-10 text-center">
            <p className="text-sm text-gray-400 mb-3">{t('dashboard.noExpenses')}</p>
            <Button variant="primary" size="sm" onClick={() => navigate('/expenses')}>{t('dashboard.addFirst')}</Button>
          </div>
        )}
      </Card>
    </PageLayout>
  )
}
