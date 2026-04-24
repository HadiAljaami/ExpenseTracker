import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { Plus, RefreshCw, Trash2, ToggleLeft, ToggleRight } from 'lucide-react'
import { useForm } from 'react-hook-form'
import PageLayout from '../../components/layout/PageLayout'
import Card from '../../components/ui/Card'
import Button from '../../components/ui/Button'
import Input from '../../components/ui/Input'
import Modal from '../../components/ui/Modal'
import Badge from '../../components/ui/Badge'
import EmptyState from '../../components/ui/EmptyState'
import { recurringApi, categoriesApi } from '../../api/index'
import { QUERY_KEYS } from '../../constants/queryKeys'
import { formatCurrency } from '../../utils/formatters'
import { useAuthStore } from '../../store/authStore'
import { clsx } from 'clsx'
import toast from 'react-hot-toast'

export default function RecurringPage() {
  const { user } = useAuthStore()
  const { t } = useTranslation()
  const qc = useQueryClient()
  const [modalOpen, setModalOpen] = useState(false)

  const { data: recurring, isLoading } = useQuery({
    queryKey: QUERY_KEYS.RECURRING,
    queryFn: () => recurringApi.getAll(),
    select: res => res.data.data,
  })
  const { data: categories } = useQuery({
    queryKey: QUERY_KEYS.CATEGORIES,
    queryFn: () => categoriesApi.getAll(),
    select: res => res.data.data,
  })

  const createMutation = useMutation({
    mutationFn: recurringApi.create,
    onSuccess: () => { qc.invalidateQueries({ queryKey: QUERY_KEYS.RECURRING }); setModalOpen(false); toast.success(t('recurring.added')) },
  })
  const toggleMutation = useMutation({
    mutationFn: recurringApi.toggle,
    onSuccess: () => qc.invalidateQueries({ queryKey: QUERY_KEYS.RECURRING }),
  })
  const deleteMutation = useMutation({
    mutationFn: recurringApi.delete,
    onSuccess: () => { qc.invalidateQueries({ queryKey: QUERY_KEYS.RECURRING }); toast.success(t('recurring.deleted')) },
  })

  const FREQ_OPTIONS = [
    { value: 'Monthly', label: t('recurring.monthly') },
    { value: 'Weekly', label: t('recurring.weekly') },
    { value: 'Yearly', label: t('recurring.yearly') },
  ]

  return (
    <PageLayout title={t('recurring.title')}
      actions={<Button variant="primary" size="sm" icon={Plus} onClick={() => setModalOpen(true)}>{t('recurring.add')}</Button>}>
      {isLoading ? (
        <div className="space-y-3">{[...Array(3)].map((_,i) => <div key={i} className="skeleton h-20 rounded-lg" />)}</div>
      ) : recurring?.length === 0 ? (
        <Card><EmptyState icon={RefreshCw} title={t('recurring.noRecurring')} description={t('recurring.noRecurringDesc')}
          action={<Button variant="primary" size="sm" icon={Plus} onClick={() => setModalOpen(true)}>{t('recurring.add')}</Button>} /></Card>
      ) : (
        <div className="space-y-3">
          {recurring?.map(item => (
            <Card key={item.id} className={clsx('flex items-center gap-4', !item.isActive && 'opacity-60')}>
              <div className="w-10 h-10 rounded-lg bg-gray-50 flex items-center justify-center text-xl flex-shrink-0">{item.categoryIcon}</div>
              <div className="flex-1 min-w-0">
                <div className="flex items-center gap-2">
                  <p className="text-sm font-semibold text-gray-900 truncate">{item.description || item.categoryName}</p>
                  <Badge variant={item.isActive ? 'green' : 'gray'}>{item.isActive ? t('recurring.active') : t('recurring.paused')}</Badge>
                </div>
                <p className="text-xs text-gray-500 mt-0.5">
                  {FREQ_OPTIONS.find(f => f.value === item.frequency)?.label || item.frequency} · {t('recurring.dayOfMonth')} {item.dayOfMonth} · {item.categoryName}
                </p>
              </div>
              <p className="text-base font-bold text-gray-900 flex-shrink-0">{formatCurrency(item.amount, user?.currency)}</p>
              <div className="flex items-center gap-1 flex-shrink-0">
                <button onClick={() => toggleMutation.mutate(item.id)}
                  className={clsx('p-1.5 rounded-lg transition-colors', item.isActive ? 'text-primary-600 hover:bg-primary-50' : 'text-gray-400 hover:bg-gray-100')}>
                  {item.isActive ? <ToggleRight size={18} /> : <ToggleLeft size={18} />}
                </button>
                <button onClick={() => deleteMutation.mutate(item.id)}
                  className="p-1.5 text-gray-400 hover:text-danger-400 hover:bg-danger-50 rounded-lg"><Trash2 size={15} /></button>
              </div>
            </Card>
          ))}
        </div>
      )}

      <Modal open={modalOpen} onClose={() => setModalOpen(false)} title={t('recurring.newRecurring')} size="sm"
        footer={<><Button variant="secondary" onClick={() => setModalOpen(false)}>{t('common.cancel')}</Button>
          <Button variant="primary" loading={createMutation.isPending}
            onClick={() => document.getElementById('rec-submit').click()}>{t('common.add')}</Button></>}>
        <form id="rec-form" onSubmit={(e) => { e.preventDefault() }} className="space-y-4">
          <div><label className="label">{t('expenses.category')}</label>
            <select id="rec-cat" className="input">
              <option value="">{t('expenses.allCategories')}</option>
              {(categories||[]).map(c => <option key={c.id} value={c.id}>{c.icon} {c.name}</option>)}
            </select>
          </div>
          <Input label={t('expenses.amount')} type="number" step="0.01" placeholder="0.00" id="rec-amount" />
          <Input label={t('expenses.description')} placeholder={t('recurring.descPlaceholder')} id="rec-desc" />
          <div><label className="label">{t('recurring.frequency')}</label>
            <select id="rec-freq" className="input">
              {FREQ_OPTIONS.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
            </select>
          </div>
          <Input label={t('recurring.dayOfMonth')} type="number" min={1} max={28} placeholder={t('recurring.dayPlaceholder')} id="rec-day" defaultValue={1} />
          <button id="rec-submit" type="button" className="hidden"
            onClick={() => {
              const cat = document.getElementById('rec-cat').value
              const amt = document.getElementById('rec-amount').value
              const desc = document.getElementById('rec-desc').value
              const freq = document.getElementById('rec-freq').value
              const day = document.getElementById('rec-day').value
              if (!cat || !amt) return
              createMutation.mutate({ categoryId: parseInt(cat), amount: parseFloat(amt), description: desc, frequency: freq, dayOfMonth: parseInt(day) })
            }} />
        </form>
      </Modal>
    </PageLayout>
  )
}
