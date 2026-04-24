import { useTranslation } from 'react-i18next'
import { useQuery } from '@tanstack/react-query'
import { Users, Receipt, Tag, TrendingUp } from 'lucide-react'
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts'
import Card, { CardHeader } from '../../components/ui/Card'
import StatCard from '../../components/ui/StatCard'
import ChartWrapper from '../../components/ui/ChartWrapper'
import { SkeletonCard } from '../../components/ui/Skeleton'
import { adminApi } from '../../api/index'
import { QUERY_KEYS } from '../../constants/queryKeys'
import { formatCurrency, formatCompact } from '../../utils/formatters'

export default function AdminStats() {
  const { t } = useTranslation()
  const { data: stats, isLoading } = useQuery({
    queryKey: QUERY_KEYS.ADMIN_STATS,
    queryFn: () => adminApi.getStats(),
    select: res => res.data.data,
  })
  const monthlyData = stats?.monthlyGrowth?.map(m => ({
    name: m.monthName, amount: m.totalAmount,
  })) || []

  if (isLoading) return (
    <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
      {[...Array(4)].map((_, i) => <SkeletonCard key={i} />)}
    </div>
  )

  return (
    <div className="space-y-4">
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard label={t('admin.totalUsers')} value={stats?.totalUsers || 0}
          icon={Users} iconColor="text-primary-400" iconBg="bg-primary-50" />
        <StatCard label={t('admin.totalExpenses')} value={formatCompact(stats?.totalExpenses || 0)}
          icon={Receipt} iconColor="text-info-400" iconBg="bg-info-50" />
        <StatCard label={t('admin.totalAmount')} value={formatCurrency(stats?.totalAmount || 0)}
          icon={TrendingUp} iconColor="text-warning-400" iconBg="bg-warning-50" />
        <StatCard label={t('admin.categories')} value={stats?.totalCategories || 0}
          icon={Tag} iconColor="text-gray-400" iconBg="bg-gray-100" />
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
        <Card>
          <CardHeader title={t('admin.monthlySpending')} />
          <ChartWrapper height={220}>
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={monthlyData} margin={{ top: 4, right: 4, left: -20, bottom: 4 }}>
                <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" vertical={false} />
                <XAxis dataKey="name" tick={{ fontSize: 11, fill: '#9CA3AF' }} axisLine={false} tickLine={false} />
                <YAxis tick={{ fontSize: 11, fill: '#9CA3AF' }} axisLine={false} tickLine={false} />
                <Tooltip cursor={{ fill: '#f9fafb' }}
                  contentStyle={{ fontSize: 12, borderRadius: 8, border: '1px solid #e5e7eb' }}
                  formatter={v => [formatCurrency(v), '']} />
                <Bar dataKey="amount" fill="#1D9E75" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </ChartWrapper>
        </Card>

        <Card>
          <CardHeader title={t('admin.topSpenders')} />
          <div className="space-y-3 mt-1">
            {stats?.topSpenders?.map((s, i) => (
              <div key={i} className="flex items-center gap-3">
                <div className="w-7 h-7 rounded-full bg-gray-100 flex items-center justify-center text-xs font-bold text-gray-500 flex-shrink-0">
                  #{i + 1}
                </div>
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium text-gray-800 truncate">{s.fullName}</p>
                  <p className="text-xs text-gray-400 truncate">{s.email}</p>
                </div>
                <div className="text-end flex-shrink-0">
                  <p className="text-sm font-bold text-gray-900">{formatCurrency(s.totalAmount)}</p>
                  <p className="text-xs text-gray-400">{s.expenseCount} txn</p>
                </div>
              </div>
            ))}
          </div>
        </Card>
      </div>
    </div>
  )
}
