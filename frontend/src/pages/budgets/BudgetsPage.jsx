import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { Plus, Trash2, Edit2, Wallet } from 'lucide-react'
import { useForm } from 'react-hook-form'
import PageLayout from '../../components/layout/PageLayout'
import Card, { CardHeader } from '../../components/ui/Card'
import Button from '../../components/ui/Button'
import Input from '../../components/ui/Input'
import Modal from '../../components/ui/Modal'
import EmptyState from '../../components/ui/EmptyState'
import { SkeletonCard } from '../../components/ui/Skeleton'
import { budgetsApi, categoriesApi } from '../../api/index'
import { QUERY_KEYS } from '../../constants/queryKeys'
import { formatCurrency, getCurrentMonthYear, getMonthName } from '../../utils/formatters'
import { useAuthStore } from '../../store/authStore'
import { clsx } from 'clsx'
import toast from 'react-hot-toast'

export default function BudgetsPage() {
  const { user } = useAuthStore()
  const { t } = useTranslation()
  const qc = useQueryClient()
  const { month, year } = getCurrentMonthYear()
  const [modalOpen, setModalOpen] = useState(false)
  const [editingBudget, setEditingBudget] = useState(null)
  const [deleteConfirm, setDeleteConfirm] = useState(null)

  const { data: budgets, isLoading } = useQuery({
    queryKey: QUERY_KEYS.BUDGETS(month, year),
    queryFn: () => budgetsApi.getAll({ month, year }),
    select: res => res.data.data,
  })
  const { data: categories } = useQuery({
    queryKey: QUERY_KEYS.CATEGORIES,
    queryFn: () => categoriesApi.getAll(),
    select: res => res.data.data,
  })

  const createMutation = useMutation({
    mutationFn: budgetsApi.create,
    onSuccess: () => { qc.invalidateQueries({ queryKey: QUERY_KEYS.BUDGETS(month, year) }); setModalOpen(false); toast.success(t('budgets.created')) },
  })
  const updateMutation = useMutation({
    mutationFn: ({ id, ...data }) => budgetsApi.update(id, data),
    onSuccess: () => { qc.invalidateQueries({ queryKey: QUERY_KEYS.BUDGETS(month, year) }); setModalOpen(false); setEditingBudget(null); toast.success(t('budgets.updated')) },
  })
  const deleteMutation = useMutation({
    mutationFn: budgetsApi.delete,
    onSuccess: () => { qc.invalidateQueries({ queryKey: QUERY_KEYS.BUDGETS(month, year) }); setDeleteConfirm(null); toast.success(t('budgets.deleted')) },
  })

  return (
    <PageLayout title={t('budgets.title')}
      actions={<Button variant="primary" size="sm" icon={Plus} onClick={() => { setEditingBudget(null); setModalOpen(true) }}>{t('budgets.addBudget')}</Button>}>
      {isLoading ? (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">{[...Array(3)].map((_, i) => <SkeletonCard key={i} />)}</div>
      ) : budgets?.length === 0 ? (
        <Card><EmptyState icon={Wallet} title={t('budgets.noBudgets')} description={t('budgets.noBudgetsDesc')}
          action={<Button variant="primary" size="sm" icon={Plus} onClick={() => setModalOpen(true)}>{t('budgets.addBudget')}</Button>} /></Card>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {budgets?.map(budget => (
            <BudgetCard key={budget.id} budget={budget} currency={user?.currency} t={t}
              onEdit={() => { setEditingBudget(budget); setModalOpen(true) }}
              onDelete={() => setDeleteConfirm(budget)} />
          ))}
        </div>
      )}

      <BudgetModal open={modalOpen} onClose={() => { setModalOpen(false); setEditingBudget(null) }}
        budget={editingBudget} categories={categories || []} month={month} year={year} t={t}
        loading={createMutation.isPending || updateMutation.isPending}
        onSubmit={(data) => {
          const payload = { ...data, month, year, monthlyLimit: parseFloat(data.monthlyLimit), categoryId: data.categoryId || null }
          if (editingBudget) updateMutation.mutate({ id: editingBudget.id, ...payload })
          else createMutation.mutate(payload)
        }} />

      <Modal open={!!deleteConfirm} onClose={() => setDeleteConfirm(null)} title={t('budgets.deleteBudget')} size="sm"
        footer={<><Button variant="secondary" onClick={() => setDeleteConfirm(null)}>{t('common.cancel')}</Button>
          <Button variant="danger" loading={deleteMutation.isPending} onClick={() => deleteMutation.mutate(deleteConfirm.id)}>{t('common.delete')}</Button></>}>
        <p className="text-sm text-gray-600" dangerouslySetInnerHTML={{ __html: t('budgets.deleteConfirm', { name: deleteConfirm?.categoryName || t('budgets.overall') }) }} />
      </Modal>
    </PageLayout>
  )
}

function BudgetCard({ budget, currency, onEdit, onDelete, t }) {
  const pct = Math.min(budget.usagePercentage, 100)
  const color = pct >= 100 ? 'bg-danger-400' : pct >= 80 ? 'bg-warning-400' : 'bg-primary-400'
  const textColor = pct >= 100 ? 'text-danger-400' : pct >= 80 ? 'text-warning-400' : 'text-primary-600'
  return (
    <Card>
      <div className="flex items-start justify-between mb-3">
        <div>
          <p className="text-sm font-semibold text-gray-900">{budget.categoryName || t('budgets.overall')}</p>
          <p className="text-xs text-gray-500 mt-0.5">{t('budgets.monthlyLimit')}</p>
        </div>
        <div className="flex gap-1">
          <button onClick={onEdit} className="p-1.5 text-gray-400 hover:text-gray-700 hover:bg-gray-100 rounded-lg"><Edit2 size={13} /></button>
          <button onClick={onDelete} className="p-1.5 text-gray-400 hover:text-danger-400 hover:bg-danger-50 rounded-lg"><Trash2 size={13} /></button>
        </div>
      </div>
      <div className="flex items-end justify-between mb-2">
        <div>
          <p className="text-xl font-bold text-gray-900">{formatCurrency(budget.totalSpent, currency)}</p>
          <p className="text-xs text-gray-500">{t('budgets.monthlyLimit')}: {formatCurrency(budget.monthlyLimit, currency)}</p>
        </div>
        <span className={clsx('text-sm font-bold', textColor)}>{pct.toFixed(0)}%</span>
      </div>
      <div className="h-2 bg-gray-100 rounded-full overflow-hidden">
        <div className={clsx('h-full rounded-full transition-all duration-700', color)} style={{ width: `${pct}%` }} />
      </div>
      <div className="flex items-center justify-between mt-2">
        <span className="text-xs text-gray-500">{t('budgets.remaining')}</span>
        <span className={clsx('text-xs font-semibold', budget.isExceeded ? 'text-danger-400' : 'text-primary-600')}>
          {budget.isExceeded ? t('budgets.overBudget') : formatCurrency(budget.remaining, currency)}
        </span>
      </div>
    </Card>
  )
}

function BudgetModal({ open, onClose, budget, categories, month, year, onSubmit, loading, t }) {
  const { register, handleSubmit, formState: { errors } } = useForm({
    defaultValues: budget ? { categoryId: budget.categoryId || '', monthlyLimit: budget.monthlyLimit } : {}
  })
  const catOptions = [{ value: '', label: t('budgets.overallAll') }, ...categories.map(c => ({ value: c.id, label: `${c.icon} ${c.name}` }))]
  return (
    <Modal open={open} onClose={onClose} title={budget ? t('budgets.editBudget') : t('budgets.newBudget')} size="sm"
      footer={<><Button variant="secondary" onClick={onClose}>{t('common.cancel')}</Button>
        <Button variant="primary" loading={loading} onClick={handleSubmit(onSubmit)}>{budget ? t('common.save') : t('common.create')}</Button></>}>
      <form className="space-y-4">
        <div><label className="label">{t('budgets.category')}</label>
          <select className="input" {...register('categoryId')}>
            {catOptions.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
          </select>
        </div>
        <Input label={t('budgets.monthlyLimit')} type="number" step="0.01" placeholder="1000"
          error={errors.monthlyLimit?.message}
          {...register('monthlyLimit', { required: t('budgets.validation.limitRequired'), min: { value: 1, message: t('budgets.validation.limitMin') } })} />
        <p className="text-xs text-gray-500">{t('budgets.budgetFor', { month: getMonthName(month, year) })}</p>
      </form>
    </Modal>
  )
}
