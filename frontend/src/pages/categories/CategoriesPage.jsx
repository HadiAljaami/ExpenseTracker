import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { Plus, Edit2, Trash2, Tag } from 'lucide-react'
import { useForm } from 'react-hook-form'
import PageLayout from '../../components/layout/PageLayout'
import Card from '../../components/ui/Card'
import Button from '../../components/ui/Button'
import Input from '../../components/ui/Input'
import Modal from '../../components/ui/Modal'
import EmptyState from '../../components/ui/EmptyState'
import { categoriesApi } from '../../api/index'
import { QUERY_KEYS } from '../../constants/queryKeys'
import toast from 'react-hot-toast'

export default function CategoriesPage() {
  const { t } = useTranslation()
  const qc = useQueryClient()
  const [modalOpen, setModalOpen] = useState(false)
  const [editingCat, setEditingCat] = useState(null)
  const [deleteConfirm, setDeleteConfirm] = useState(null)

  const { data: categories, isLoading } = useQuery({
    queryKey: QUERY_KEYS.CATEGORIES,
    queryFn: () => categoriesApi.getAll(),
    select: res => res.data.data,
  })

  const createMutation = useMutation({
    mutationFn: categoriesApi.create,
    onSuccess: () => { qc.invalidateQueries({ queryKey: QUERY_KEYS.CATEGORIES }); setModalOpen(false); toast.success(t('categories.created')) },
  })
  const updateMutation = useMutation({
    mutationFn: ({ id, ...data }) => categoriesApi.update(id, data),
    onSuccess: () => { qc.invalidateQueries({ queryKey: QUERY_KEYS.CATEGORIES }); setModalOpen(false); setEditingCat(null); toast.success(t('categories.updated')) },
  })
  const deleteMutation = useMutation({
    mutationFn: categoriesApi.delete,
    onSuccess: () => { qc.invalidateQueries({ queryKey: QUERY_KEYS.CATEGORIES }); setDeleteConfirm(null); toast.success(t('categories.deleted')) },
  })

  return (
    <PageLayout title={t('categories.title')}
      actions={<Button variant="primary" size="sm" icon={Plus} onClick={() => { setEditingCat(null); setModalOpen(true) }}>{t('categories.addCategory')}</Button>}>
      {isLoading ? (
        <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-3">
          {[...Array(8)].map((_, i) => <div key={i} className="skeleton h-24 rounded-lg" />)}
        </div>
      ) : categories?.length === 0 ? (
        <Card><EmptyState icon={Tag} title={t('categories.noCategories')} description={t('categories.noCategoriesDesc')}
          action={<Button variant="primary" size="sm" icon={Plus} onClick={() => setModalOpen(true)}>{t('categories.addCategory')}</Button>} /></Card>
      ) : (
        <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 gap-3">
          {categories?.map(cat => (
            <Card key={cat.id} className="text-center relative group cursor-pointer hover:shadow-md transition-shadow">
              <div className="flex flex-col items-center gap-2 py-2">
                <div className="w-12 h-12 rounded-xl flex items-center justify-center text-2xl" style={{ background: cat.color + '20' }}>{cat.icon}</div>
                <p className="text-sm font-medium text-gray-800 truncate w-full px-1">{cat.name}</p>
                <div className="w-3 h-3 rounded-full" style={{ background: cat.color }} />
              </div>
              <div className="absolute top-2 end-2 hidden group-hover:flex gap-1">
                <button onClick={() => { setEditingCat(cat); setModalOpen(true) }} className="p-1 text-gray-400 hover:text-gray-700 bg-white rounded shadow-sm"><Edit2 size={12} /></button>
                <button onClick={() => setDeleteConfirm(cat)} className="p-1 text-gray-400 hover:text-danger-400 bg-white rounded shadow-sm"><Trash2 size={12} /></button>
              </div>
            </Card>
          ))}
        </div>
      )}

      <CategoryModal open={modalOpen} onClose={() => { setModalOpen(false); setEditingCat(null) }} t={t}
        category={editingCat} loading={createMutation.isPending || updateMutation.isPending}
        onSubmit={(data) => editingCat ? updateMutation.mutate({ id: editingCat.id, ...data }) : createMutation.mutate(data)} />

      <Modal open={!!deleteConfirm} onClose={() => setDeleteConfirm(null)} title={t('categories.newCategory')} size="sm"
        footer={<><Button variant="secondary" onClick={() => setDeleteConfirm(null)}>{t('common.cancel')}</Button>
          <Button variant="danger" loading={deleteMutation.isPending} onClick={() => deleteMutation.mutate(deleteConfirm.id)}>{t('common.delete')}</Button></>}>
        <p className="text-sm text-gray-600" dangerouslySetInnerHTML={{ __html: t('categories.deleteConfirm', { name: deleteConfirm?.name || '' }) }} />
      </Modal>
    </PageLayout>
  )
}

function CategoryModal({ open, onClose, category, onSubmit, loading, t }) {
  const { register, handleSubmit, formState: { errors } } = useForm({
    defaultValues: category ? { name: category.name, icon: category.icon, color: category.color } : { color: '#1D9E75' }
  })
  return (
    <Modal open={open} onClose={onClose} title={category ? t('categories.editCategory') : t('categories.newCategory')} size="sm"
      footer={<><Button variant="secondary" onClick={onClose}>{t('common.cancel')}</Button>
        <Button variant="primary" loading={loading} onClick={handleSubmit(onSubmit)}>{category ? t('common.save') : t('common.create')}</Button></>}>
      <form className="space-y-4">
        <Input label={t('categories.name')} placeholder={t('categories.namePlaceholder')} error={errors.name?.message}
          {...register('name', { required: true })} />
        <Input label={t('categories.icon')} placeholder={t('categories.iconPlaceholder')} error={errors.icon?.message}
          {...register('icon', { required: true })} />
        <div><label className="label">{t('categories.color')}</label>
          <input type="color" className="w-full h-10 rounded-lg border border-gray-200 cursor-pointer p-1" {...register('color')} /></div>
      </form>
    </Modal>
  )
}
