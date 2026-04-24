import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { Plus, Search, Filter, Trash2, Edit2, RotateCcw, ChevronLeft, ChevronRight } from 'lucide-react'
import { useForm } from 'react-hook-form'
import PageLayout from '../../components/layout/PageLayout'
import Card from '../../components/ui/Card'
import Button from '../../components/ui/Button'
import Input from '../../components/ui/Input'
import Modal from '../../components/ui/Modal'
import EmptyState from '../../components/ui/EmptyState'
import { SkeletonTable } from '../../components/ui/Skeleton'
import { expensesApi, categoriesApi } from '../../api/index'
import { QUERY_KEYS } from '../../constants/queryKeys'
import { formatCurrency, formatDate } from '../../utils/formatters'
import { useAuthStore } from '../../store/authStore'
import toast from 'react-hot-toast'

export default function ExpensesPage() {
  const { user } = useAuthStore()
  const { t } = useTranslation()
  const qc = useQueryClient()
  const [page, setPage] = useState(1)
  const [search, setSearch] = useState('')
  const [categoryId, setCategoryId] = useState('')
  const [sort, setSort] = useState('Date_desc')
  const [showDeleted, setShowDeleted] = useState(false)
  const [modalOpen, setModalOpen] = useState(false)
  const [editingExpense, setEditingExpense] = useState(null)
  const [deleteConfirm, setDeleteConfirm] = useState(null)

  const [sortBy, sortDirection] = sort.split('_')

  const { data: categoriesData } = useQuery({
    queryKey: QUERY_KEYS.CATEGORIES,
    queryFn: () => categoriesApi.getAll(),
    select: res => res.data.data,
  })

  const { data, isLoading } = useQuery({
    queryKey: [...QUERY_KEYS.EXPENSES, { page, search, categoryId, sortBy, sortDirection }],
    queryFn: () => expensesApi.getAll({ page, pageSize: 10, search, categoryId: categoryId || undefined, sortBy, sortDirection }),
    select: res => res.data.data,
    keepPreviousData: true,
  })

  const { data: deletedData } = useQuery({
    queryKey: QUERY_KEYS.DELETED_EXPENSES,
    queryFn: () => expensesApi.getDeleted(),
    select: res => res.data.data,
    enabled: showDeleted,
  })

  const createMutation = useMutation({
    mutationFn: expensesApi.create,
    onSuccess: () => { qc.invalidateQueries({ queryKey: QUERY_KEYS.EXPENSES }); setModalOpen(false); toast.success(t('expenses.expenseAdded')) },
  })
  const updateMutation = useMutation({
    mutationFn: ({ id, ...data }) => expensesApi.update(id, data),
    onSuccess: () => { qc.invalidateQueries({ queryKey: QUERY_KEYS.EXPENSES }); setModalOpen(false); setEditingExpense(null); toast.success(t('expenses.expenseUpdated')) },
  })
  const deleteMutation = useMutation({
    mutationFn: expensesApi.delete,
    onSuccess: () => { qc.invalidateQueries({ queryKey: QUERY_KEYS.EXPENSES }); setDeleteConfirm(null); toast.success(t('expenses.expenseDeleted')) },
  })
  const restoreMutation = useMutation({
    mutationFn: expensesApi.restore,
    onSuccess: () => { qc.invalidateQueries({ queryKey: QUERY_KEYS.EXPENSES }); qc.invalidateQueries({ queryKey: QUERY_KEYS.DELETED_EXPENSES }); toast.success(t('expenses.expenseRestored')) },
  })

  const categoryOptions = (categoriesData || []).map(c => ({ value: c.id, label: `${c.icon} ${c.name}` }))
  const displayedExpenses = showDeleted ? (deletedData || []) : (data?.items || [])

  const SORT_OPTIONS = [
    { value: 'Date_desc', label: t('expenses.newestFirst') },
    { value: 'Date_asc', label: t('expenses.oldestFirst') },
    { value: 'Amount_desc', label: t('expenses.highestAmount') },
    { value: 'Amount_asc', label: t('expenses.lowestAmount') },
  ]

  return (
    <PageLayout title={t('expenses.title')}
      actions={<Button variant="primary" size="sm" icon={Plus} onClick={() => { setEditingExpense(null); setModalOpen(true) }}>{t('expenses.addExpense')}</Button>}>

      <Card className="mb-4">
        <div className="flex flex-col sm:flex-row gap-3">
          <div className="flex-1 relative">
            <Search size={15} className="absolute start-3 top-1/2 -translate-y-1/2 text-gray-400" />
            <input className="input ps-9 text-sm" placeholder={t('common.search')} value={search}
              onChange={e => { setSearch(e.target.value); setPage(1) }} />
          </div>
          <select className="input sm:w-44 text-sm" value={categoryId} onChange={e => { setCategoryId(e.target.value); setPage(1) }}>
            <option value="">{t('expenses.allCategories')}</option>
            {categoryOptions.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
          </select>
          <select className="input sm:w-44 text-sm" value={sort} onChange={e => setSort(e.target.value)}>
            {SORT_OPTIONS.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
          </select>
          <Button variant={showDeleted ? 'danger' : 'secondary'} size="sm" onClick={() => setShowDeleted(!showDeleted)}>
            {showDeleted ? t('expenses.showActive') : t('expenses.showDeleted')}
          </Button>
        </div>
      </Card>

      <Card padding={false}>
        {isLoading ? (
          <div className="p-4"><SkeletonTable rows={8} /></div>
        ) : displayedExpenses.length === 0 ? (
          <EmptyState icon={Filter}
            title={showDeleted ? t('expenses.noDeleted') : t('expenses.noExpenses')}
            description={showDeleted ? t('expenses.deletedDesc') : t('expenses.addFirst')}
            action={!showDeleted && <Button variant="primary" size="sm" icon={Plus} onClick={() => { setEditingExpense(null); setModalOpen(true) }}>{t('expenses.addExpense')}</Button>} />
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="table">
                <thead>
                  <tr>
                    <th>{t('expenses.description')}</th>
                    <th>{t('expenses.category')}</th>
                    <th>{t('expenses.date')}</th>
                    <th className="text-end">{t('expenses.amount')}</th>
                    <th className="text-end">{t('common.actions')}</th>
                  </tr>
                </thead>
                <tbody>
                  {displayedExpenses.map(expense => (
                    <tr key={expense.id}>
                      <td><p className="font-medium text-gray-900 truncate max-w-[200px]">{expense.description || '—'}</p></td>
                      <td>
                        <div className="flex items-center gap-2">
                          <span className="text-base">{expense.categoryIcon}</span>
                          <span className="text-sm text-gray-600">{expense.categoryName}</span>
                        </div>
                      </td>
                      <td className="text-gray-500 text-sm whitespace-nowrap">{formatDate(expense.date)}</td>
                      <td className="text-end font-semibold text-gray-900 whitespace-nowrap">{formatCurrency(expense.amount, user?.currency)}</td>
                      <td className="text-end">
                        <div className="flex items-center justify-end gap-1">
                          {showDeleted ? (
                            <button onClick={() => restoreMutation.mutate(expense.id)} title={t('expenses.restore')}
                              className="p-1.5 text-primary-600 hover:bg-primary-50 rounded-lg"><RotateCcw size={14} /></button>
                          ) : (
                            <>
                              <button onClick={() => { setEditingExpense(expense); setModalOpen(true) }} title={t('common.edit')}
                                className="p-1.5 text-gray-400 hover:text-gray-700 hover:bg-gray-100 rounded-lg"><Edit2 size={14} /></button>
                              <button onClick={() => setDeleteConfirm(expense)} title={t('common.delete')}
                                className="p-1.5 text-gray-400 hover:text-danger-400 hover:bg-danger-50 rounded-lg"><Trash2 size={14} /></button>
                            </>
                          )}
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            {!showDeleted && data?.totalPages > 1 && (
              <div className="flex items-center justify-between px-4 py-3 border-t border-gray-100">
                <p className="text-xs text-gray-500">{((page-1)*10)+1}–{Math.min(page*10, data.totalCount)} / {data.totalCount}</p>
                <div className="flex items-center gap-1">
                  <button onClick={() => setPage(p => Math.max(1, p-1))} disabled={page===1}
                    className="p-1.5 rounded-lg hover:bg-gray-100 disabled:opacity-40 text-gray-600"><ChevronLeft size={16} /></button>
                  {[...Array(Math.min(5, data.totalPages))].map((_, i) => (
                    <button key={i+1} onClick={() => setPage(i+1)}
                      className={`w-7 h-7 text-xs rounded-lg font-medium ${i+1===page ? 'bg-primary-400 text-white' : 'text-gray-600 hover:bg-gray-100'}`}>{i+1}</button>
                  ))}
                  <button onClick={() => setPage(p => Math.min(data.totalPages, p+1))} disabled={page===data.totalPages}
                    className="p-1.5 rounded-lg hover:bg-gray-100 disabled:opacity-40 text-gray-600"><ChevronRight size={16} /></button>
                </div>
              </div>
            )}
          </>
        )}
      </Card>

      <ExpenseModal open={modalOpen} onClose={() => { setModalOpen(false); setEditingExpense(null) }}
        expense={editingExpense} categories={categoriesData || []} t={t}
        onSubmit={(data) => editingExpense ? updateMutation.mutate({ id: editingExpense.id, ...data }) : createMutation.mutate(data)}
        loading={createMutation.isPending || updateMutation.isPending} />

      <Modal open={!!deleteConfirm} onClose={() => setDeleteConfirm(null)} title={t('expenses.deleteExpense')} size="sm"
        footer={<><Button variant="secondary" onClick={() => setDeleteConfirm(null)}>{t('common.cancel')}</Button>
          <Button variant="danger" loading={deleteMutation.isPending} onClick={() => deleteMutation.mutate(deleteConfirm.id)}>{t('common.delete')}</Button></>}>
        <p className="text-sm text-gray-600" dangerouslySetInnerHTML={{ __html: t('expenses.deleteConfirm', { name: deleteConfirm?.description || '' }) }} />
      </Modal>
    </PageLayout>
  )
}

function ExpenseModal({ open, onClose, expense, categories, onSubmit, loading, t }) {
  const { register, handleSubmit, formState: { errors } } = useForm({
    defaultValues: expense ? { categoryId: expense.categoryId, amount: expense.amount, description: expense.description, date: expense.date?.split('T')[0] } : { date: new Date().toISOString().split('T')[0] }
  })
  const categoryOptions = categories.map(c => ({ value: c.id, label: `${c.icon} ${c.name}` }))
  return (
    <Modal open={open} onClose={onClose} title={expense ? t('expenses.editExpense') : t('expenses.addExpense')} size="sm"
      footer={<><Button variant="secondary" onClick={onClose}>{t('common.cancel')}</Button>
        <Button variant="primary" loading={loading} onClick={handleSubmit(onSubmit)}>{expense ? t('common.save') : t('expenses.addExpense')}</Button></>}>
      <form className="space-y-4">
        <div><label className="label">{t('expenses.category')}</label>
          <select className="input" {...register('categoryId', { required: t('expenses.validation.categoryRequired') })}>
            <option value="">{t('expenses.allCategories')}</option>
            {categoryOptions.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
          </select>
          {errors.categoryId && <p className="error-text">{errors.categoryId.message}</p>}
        </div>
        <Input label={t('expenses.amount')} type="number" step="0.01" placeholder="0.00"
          error={errors.amount?.message}
          {...register('amount', { required: t('expenses.validation.amountRequired'), min: { value: 0.01, message: t('expenses.validation.amountMin') } })} />
        <Input label={t('expenses.description')} type="text" placeholder={t('expenses.whatSpend')} {...register('description')} />
        <Input label={t('expenses.date')} type="date" error={errors.date?.message}
          {...register('date', { required: t('expenses.validation.dateRequired') })} />
      </form>
    </Modal>
  )
}
