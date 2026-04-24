import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { TrendingUp, TrendingDown, ArrowUp, ArrowDown } from 'lucide-react'
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, PieChart, Pie, Cell } from 'recharts'
import PageLayout from '../../components/layout/PageLayout'
import Card, { CardHeader } from '../../components/ui/Card'
import StatCard from '../../components/ui/StatCard'
import ChartWrapper from '../../components/ui/ChartWrapper'
import { SkeletonCard } from '../../components/ui/Skeleton'
import { insightsApi } from '../../api/index'
import { QUERY_KEYS } from '../../constants/queryKeys'
import { formatCurrency, formatDateShort, getMonthName, getCurrentMonthYear } from '../../utils/formatters'
import { useAuthStore } from '../../store/authStore'

const COLORS = ['#1D9E75','#378ADD','#BA7517','#E24B4A','#7F77DD','#DDA0DD','#FF6B6B','#B0BEC5']
const MONTHS = Array.from({ length: 12 }, (_, i) => ({ value: i+1, label: new Date(2024,i).toLocaleString('en',{month:'long'}) }))
const YEARS  = [2024, 2025, 2026, 2027]

export default function InsightsPage() {
  const { user } = useAuthStore()
  const { t } = useTranslation()
  const { month: curMonth, year: curYear } = getCurrentMonthYear()
  const [month, setMonth] = useState(curMonth)
  const [year, setYear]   = useState(curYear)

  const { data: insights, isLoading } = useQuery({
    queryKey: QUERY_KEYS.INSIGHTS(month, year),
    queryFn: () => insightsApi.getSummary({ month, year }),
    select: res => res.data.data,
  })

  if (isLoading) return (
    <PageLayout title={t('insights.title')}>
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
        {[...Array(4)].map((_, i) => <SkeletonCard key={i} />)}
      </div>
    </PageLayout>
  )

  const dailyData = insights?.dailyTrend?.map(d => ({ date: formatDateShort(d.date), amount: d.amount })) || []
  const pieData   = insights?.categoryBreakdown?.map(c => ({ name: c.categoryName, value: c.totalAmount, icon: c.categoryIcon })) || []

  return (
    <PageLayout title={t('insights.title')}
      actions={
        <div className="flex items-center gap-2">
          <select className="input text-sm py-1.5 w-36" value={month} onChange={e => setMonth(Number(e.target.value))}>
            {MONTHS.map(m => <option key={m.value} value={m.value}>{m.label}</option>)}
          </select>
          <select className="input text-sm py-1.5 w-24" value={year} onChange={e => setYear(Number(e.target.value))}>
            {YEARS.map(y => <option key={y} value={y}>{y}</option>)}
          </select>
        </div>
      }>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-5">
        <StatCard label={t('insights.totalThisMonth')} value={formatCurrency(insights?.totalThisMonth||0, user?.currency)}
          change={insights?.changePercentage} changeLabel={t('insights.vsLastMonth')}
          icon={TrendingUp} iconColor="text-primary-400" iconBg="bg-primary-50" />
        <StatCard label={t('insights.lastMonth')} value={formatCurrency(insights?.totalLastMonth||0, user?.currency)}
          icon={TrendingDown} iconColor="text-gray-400" iconBg="bg-gray-100" />
        <StatCard label={t('insights.dailyAverage')} value={formatCurrency(insights?.averageDailySpending||0, user?.currency)}
          changeLabel={getMonthName(month, year)} icon={ArrowUp} iconColor="text-info-400" iconBg="bg-info-50" />
        <StatCard label={t('insights.topCategory')} value={insights?.highestSpendingCategory||'—'}
          changeLabel={formatCurrency(insights?.highestCategoryAmount||0, user?.currency)}
          icon={ArrowDown} iconColor="text-warning-400" iconBg="bg-warning-50" />
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4 mb-4">
        <Card className="lg:col-span-2">
          <CardHeader title={t('insights.dailyTrend')} />
          {dailyData.length > 0 ? (
            <ChartWrapper height={220}>
              <ResponsiveContainer width="100%" height="100%">
                <LineChart data={dailyData} margin={{ top: 4, right: 4, left: -20, bottom: 20 }}>
                  <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" vertical={false} />
                  <XAxis dataKey="date" tick={{ fontSize: 11, fill: '#9CA3AF' }} axisLine={false} tickLine={false} />
                  <YAxis tick={{ fontSize: 11, fill: '#9CA3AF' }} axisLine={false} tickLine={false} />
                  <Tooltip cursor={{ stroke: '#e5e7eb' }} contentStyle={{ fontSize: 12, borderRadius: 8, border: '1px solid #e5e7eb' }}
                    formatter={v => [formatCurrency(v, user?.currency), '']} />
                  <Line type="monotone" dataKey="amount" stroke="#1D9E75" strokeWidth={2.5}
                    dot={{ r: 3, fill: '#1D9E75', strokeWidth: 0 }} activeDot={{ r: 5, strokeWidth: 0 }} />
                </LineChart>
              </ResponsiveContainer>
            </ChartWrapper>
          ) : <div className="h-48 flex items-center justify-center text-sm text-gray-400">{t('common.noData')}</div>}
        </Card>

        <Card>
          <CardHeader title={t('insights.breakdown')} />
          {pieData.length > 0 ? (
            <>
              <ChartWrapper height={160}>
                <ResponsiveContainer width="100%" height="100%">
                  <PieChart>
                    <Pie data={pieData} cx="50%" cy="50%" innerRadius={48} outerRadius={72}
                      dataKey="value" paddingAngle={2} strokeWidth={0}>
                      {pieData.map((_, i) => <Cell key={i} fill={COLORS[i%COLORS.length]} />)}
                    </Pie>
                    <Tooltip contentStyle={{ fontSize: 12, borderRadius: 8, border: '1px solid #e5e7eb' }}
                      formatter={v => [formatCurrency(v, user?.currency), '']} />
                  </PieChart>
                </ResponsiveContainer>
              </ChartWrapper>
              <div className="space-y-2 mt-3">
                {pieData.slice(0, 5).map((item, i) => (
                  <div key={i} className="flex items-center gap-2">
                    <div className="w-2.5 h-2.5 rounded-full flex-shrink-0" style={{ background: COLORS[i%COLORS.length] }} />
                    <span className="text-xs text-gray-600 flex-1 truncate">{item.icon} {item.name}</span>
                    <span className="text-xs font-semibold">{formatCurrency(item.value, user?.currency)}</span>
                  </div>
                ))}
              </div>
            </>
          ) : <div className="h-48 flex items-center justify-center text-sm text-gray-400">{t('common.noData')}</div>}
        </Card>
      </div>

      <Card>
        <CardHeader title={t('insights.categoryDetails')} />
        {insights?.categoryBreakdown?.length > 0 ? (
          <div className="space-y-3">
            {insights.categoryBreakdown.map((cat, i) => (
              <div key={i} className="flex items-center gap-3">
                <div className="w-8 h-8 rounded-lg bg-gray-50 flex items-center justify-center text-sm flex-shrink-0">{cat.categoryIcon}</div>
                <div className="flex-1 min-w-0">
                  <div className="flex items-center justify-between mb-1">
                    <span className="text-sm font-medium text-gray-800">{cat.categoryName}</span>
                    <div className="flex items-center gap-3 flex-shrink-0 ms-2">
                      <span className="text-xs text-gray-400">{cat.transactionCount} {t('insights.transactions')}</span>
                      <span className="text-sm font-semibold text-gray-900">{formatCurrency(cat.totalAmount, user?.currency)}</span>
                    </div>
                  </div>
                  <div className="h-1.5 bg-gray-100 rounded-full overflow-hidden">
                    <div className="h-full rounded-full" style={{ width: `${cat.percentage}%`, background: COLORS[i%COLORS.length] }} />
                  </div>
                </div>
                <span className="text-xs font-medium text-gray-500 w-10 text-end flex-shrink-0">{cat.percentage.toFixed(1)}%</span>
              </div>
            ))}
          </div>
        ) : <p className="text-sm text-gray-400 text-center py-8">{t('insights.noExpenses')}</p>}
      </Card>
    </PageLayout>
  )
}
