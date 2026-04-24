import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { Download } from 'lucide-react'
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts'
import PageLayout from '../../components/layout/PageLayout'
import Card, { CardHeader } from '../../components/ui/Card'
import Button from '../../components/ui/Button'
import ChartWrapper from '../../components/ui/ChartWrapper'
import { SkeletonCard } from '../../components/ui/Skeleton'
import { reportsApi } from '../../api/index'
import { QUERY_KEYS } from '../../constants/queryKeys'
import { formatCurrency, getCurrentMonthYear } from '../../utils/formatters'
import { useAuthStore } from '../../store/authStore'
import { clsx } from 'clsx'
import toast from 'react-hot-toast'

const MONTHS = Array.from({ length: 12 }, (_, i) => ({ value: i+1, label: new Date(2024,i).toLocaleString('en',{month:'long'}) }))
const YEARS  = [2024, 2025, 2026, 2027]
const COLORS = ['#1D9E75','#378ADD','#BA7517','#E24B4A','#7F77DD','#DDA0DD','#FF6B6B','#B0BEC5']

export default function ReportsPage() {
  const { user } = useAuthStore()
  const { t } = useTranslation()
  const { month: curMonth, year: curYear } = getCurrentMonthYear()
  const [month, setMonth] = useState(curMonth)
  const [year, setYear]   = useState(curYear)
  const [tab, setTab]     = useState('monthly')

  const { data: monthlyReport, isLoading: mLoading } = useQuery({
    queryKey: QUERY_KEYS.MONTHLY_REPORT(month, year),
    queryFn: () => reportsApi.getMonthly({ month, year }),
    select: res => res.data.data,
    enabled: tab === 'monthly',
  })
  const { data: yearlyReport, isLoading: yLoading } = useQuery({
    queryKey: QUERY_KEYS.YEARLY_REPORT(year),
    queryFn: () => reportsApi.getYearly({ year }),
    select: res => res.data.data,
    enabled: tab === 'yearly',
  })

  const handleExport = async () => {
    try {
      const res = await reportsApi.exportExcel({ month, year })
      const url = URL.createObjectURL(new Blob([res.data]))
      const a = document.createElement('a')
      a.href = url; a.download = `Expenses_${month}_${year}.xlsx`; a.click()
      URL.revokeObjectURL(url)
      toast.success(t('reports.exportSuccess'))
    } catch { toast.error(t('reports.exportFailed')) }
  }

  return (
    <PageLayout title={t('reports.title')}
      actions={
        <div className="flex items-center gap-2">
          {tab === 'monthly' && (
            <select className="input text-sm py-1.5 w-36" value={month} onChange={e => setMonth(Number(e.target.value))}>
              {MONTHS.map(m => <option key={m.value} value={m.value}>{m.label}</option>)}
            </select>
          )}
          <select className="input text-sm py-1.5 w-24" value={year} onChange={e => setYear(Number(e.target.value))}>
            {YEARS.map(y => <option key={y} value={y}>{y}</option>)}
          </select>
          <Button variant="secondary" size="sm" icon={Download} onClick={handleExport}>{t('common.export')}</Button>
        </div>
      }>

      {/* Tabs */}
      <div className="flex gap-1 p-1 bg-gray-100 rounded-xl w-fit mb-5">
        {['monthly','yearly'].map(t_item => (
          <button key={t_item} onClick={() => setTab(t_item)}
            className={clsx('px-5 py-1.5 rounded-lg text-sm font-medium transition-all',
              tab === t_item ? 'bg-white text-gray-900 shadow-sm' : 'text-gray-500 hover:text-gray-700')}>
            {t(`reports.${t_item}`)}
          </button>
        ))}
      </div>

      {tab === 'monthly' && (mLoading
        ? <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">{[...Array(3)].map((_,i) => <SkeletonCard key={i} />)}</div>
        : <MonthlyReport report={monthlyReport} currency={user?.currency} t={t} />
      )}
      {tab === 'yearly' && (yLoading
        ? <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">{[...Array(3)].map((_,i) => <SkeletonCard key={i} />)}</div>
        : <YearlyReport report={yearlyReport} currency={user?.currency} t={t} />
      )}
    </PageLayout>
  )
}

function MonthlyReport({ report, currency, t }) {
  if (!report) return <p className="text-gray-400 text-center py-16">{t('common.noData')}</p>
  const weeklyData = report.weeklyBreakdown?.map(w => ({ name: `Week ${w.weekNumber}`, amount: w.amount })) || []
  return (
    <div className="space-y-4">
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <Card className="text-center p-5">
          <p className="text-xs font-medium text-gray-500 mb-1">{t('reports.totalExpenses')}</p>
          <p className="text-2xl font-bold text-gray-900">{formatCurrency(report.totalExpenses, currency)}</p>
        </Card>
        <Card className="text-center p-5">
          <p className="text-xs font-medium text-gray-500 mb-1">{t('reports.budget')}</p>
          <p className="text-2xl font-bold text-gray-900">{formatCurrency(report.totalBudget, currency)}</p>
        </Card>
        <Card className={clsx('text-center p-5', report.overBudget && 'border-danger-400')}>
          <p className="text-xs font-medium text-gray-500 mb-1">{t('reports.remaining')}</p>
          <p className={clsx('text-2xl font-bold', report.overBudget ? 'text-danger-400' : 'text-primary-600')}>
            {report.overBudget ? t('reports.overBudget') : formatCurrency(report.budgetRemaining, currency)}
          </p>
        </Card>
      </div>
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
        <Card>
          <CardHeader title={t('reports.weeklyBreakdown')} />
          <ChartWrapper height={200}>
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={weeklyData} margin={{ top: 4, right: 4, left: -20, bottom: 4 }}>
                <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" vertical={false} />
                <XAxis dataKey="name" tick={{ fontSize: 11, fill: '#9CA3AF' }} axisLine={false} tickLine={false} />
                <YAxis tick={{ fontSize: 11, fill: '#9CA3AF' }} axisLine={false} tickLine={false} />
                <Tooltip cursor={{ fill: '#f9fafb' }} contentStyle={{ fontSize: 12, borderRadius: 8, border: '1px solid #e5e7eb' }}
                  formatter={v => [formatCurrency(v, currency), '']} />
                <Bar dataKey="amount" fill="#1D9E75" radius={[4,4,0,0]} />
              </BarChart>
            </ResponsiveContainer>
          </ChartWrapper>
        </Card>
        <Card>
          <CardHeader title={t('reports.categoryBreakdown')} />
          <div className="space-y-3 mt-1">
            {report.categories?.slice(0,6).map((cat,i) => (
              <div key={i} className="flex items-center gap-3">
                <span className="text-base flex-shrink-0">{cat.categoryIcon}</span>
                <div className="flex-1 min-w-0">
                  <div className="flex justify-between mb-1">
                    <span className="text-xs font-medium text-gray-700 truncate">{cat.categoryName}</span>
                    <span className="text-xs font-semibold text-gray-900 ms-2 flex-shrink-0">{formatCurrency(cat.amount, currency)}</span>
                  </div>
                  <div className="h-1.5 bg-gray-100 rounded-full overflow-hidden">
                    <div className="h-full rounded-full" style={{ width:`${cat.percentage}%`, background: COLORS[i%COLORS.length] }} />
                  </div>
                </div>
                <span className="text-xs text-gray-400 w-10 text-end flex-shrink-0">{cat.percentage.toFixed(1)}%</span>
              </div>
            ))}
          </div>
        </Card>
      </div>
    </div>
  )
}

function YearlyReport({ report, currency, t }) {
  if (!report) return <p className="text-gray-400 text-center py-16">{t('common.noData')}</p>
  const monthlyData = report.monthlyBreakdown?.map(m => ({ name: m.monthName?.slice(0,3), amount: m.totalExpenses })) || []
  return (
    <div className="space-y-4">
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <Card className="text-center p-5">
          <p className="text-xs font-medium text-gray-500 mb-1">{t('reports.total', { year: report.year })}</p>
          <p className="text-2xl font-bold text-gray-900">{formatCurrency(report.totalExpenses, currency)}</p>
        </Card>
        <Card className="text-center p-5">
          <p className="text-xs font-medium text-gray-500 mb-1">{t('reports.monthlyAverage')}</p>
          <p className="text-2xl font-bold text-gray-900">{formatCurrency(report.averageMonthlySpending, currency)}</p>
        </Card>
        <Card className="text-center p-5">
          <p className="text-xs font-medium text-gray-500 mb-1">{t('reports.highestMonth')}</p>
          <p className="text-2xl font-bold text-primary-600">{report.highestSpendingMonth}</p>
        </Card>
      </div>
      <Card>
        <CardHeader title={t('reports.monthlySpending', { year: report.year })} />
        <ChartWrapper height={240}>
          <ResponsiveContainer width="100%" height="100%">
            <BarChart data={monthlyData} margin={{ top: 4, right: 4, left: -20, bottom: 4 }}>
              <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" vertical={false} />
              <XAxis dataKey="name" tick={{ fontSize: 11, fill: '#9CA3AF' }} axisLine={false} tickLine={false} />
              <YAxis tick={{ fontSize: 11, fill: '#9CA3AF' }} axisLine={false} tickLine={false} />
              <Tooltip cursor={{ fill: '#f9fafb' }} contentStyle={{ fontSize: 12, borderRadius: 8, border: '1px solid #e5e7eb' }}
                formatter={v => [formatCurrency(v, currency), '']} />
              <Bar dataKey="amount" fill="#1D9E75" radius={[4,4,0,0]} />
            </BarChart>
          </ResponsiveContainer>
        </ChartWrapper>
      </Card>
    </div>
  )
}
