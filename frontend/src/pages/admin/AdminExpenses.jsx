import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { Search, Trash2 } from 'lucide-react'
import Card from '../../components/ui/Card'
import { SkeletonTable } from '../../components/ui/Skeleton'
import { adminApi } from '../../api/index'
import { QUERY_KEYS } from '../../constants/queryKeys'
import { formatCurrency, formatDate } from '../../utils/formatters'
import toast from 'react-hot-toast'

export default function AdminExpenses() {
  const { t } = useTranslation()
  const qc = useQueryClient()
  const [search, setSearch] = useState('')

  const { data, isLoading } = useQuery({
    queryKey: [...QUERY_KEYS.ADMIN_EXPENSES, search],
    queryFn: () => adminApi.getExpenses({ search, pageSize: 50 }),
    select: res => res.data.data,
  })

  const deleteMutation = useMutation({
    mutationFn: adminApi.deleteExpense,
    onSuccess: () => { qc.invalidateQueries({ queryKey: QUERY_KEYS.ADMIN_EXPENSES }); toast.success(t('admin.expenseDeleted')) },
  })

  const expenses = data?.items || []

  return (
    <div>
      <div className="flex gap-3 mb-4">
        <div className="relative flex-1">
          <Search size={15} className="absolute start-3 top-1/2 -translate-y-1/2 text-gray-400" />
          <input className="input ps-9 text-sm" placeholder={t('admin.searchExpenses')} value={search} onChange={e => setSearch(e.target.value)} />
        </div>
        {data && <p className="text-sm text-gray-500 self-center whitespace-nowrap">{t('admin.total')} {formatCurrency(data.totalAmount)}</p>}
      </div>
      <Card padding={false}>
        {isLoading ? <div className="p-4"><SkeletonTable rows={6} /></div> : (
          <div className="overflow-x-auto">
            <table className="table">
              <thead><tr>
                <th>User</th>
                <th>{t('expenses.description')}</th>
                <th>{t('expenses.category')}</th>
                <th>{t('expenses.date')}</th>
                <th className="text-end">{t('expenses.amount')}</th>
                <th />
              </tr></thead>
              <tbody>
                {expenses.map(e => (
                  <tr key={e.id}>
                    <td><p className="text-xs font-medium text-gray-700 truncate max-w-[120px]">{e.userFullName}</p><p className="text-xs text-gray-400 truncate max-w-[120px]">{e.userEmail}</p></td>
                    <td><p className="text-sm text-gray-700 truncate max-w-[150px]">{e.description||'—'}</p></td>
                    <td><span className="text-base me-1">{e.categoryIcon}</span><span className="text-sm text-gray-600">{e.categoryName}</span></td>
                    <td className="text-sm text-gray-500 whitespace-nowrap">{formatDate(e.date)}</td>
                    <td className="text-end font-semibold text-gray-900 whitespace-nowrap">{formatCurrency(e.amount)}</td>
                    <td><button onClick={() => { if(confirm('Delete?')) deleteMutation.mutate(e.id) }}
                      className="p-1.5 text-gray-400 hover:text-danger-400 hover:bg-danger-50 rounded-lg"><Trash2 size={14} /></button></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </Card>
    </div>
  )
}
