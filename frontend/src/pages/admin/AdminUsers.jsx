import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { Search, Plus, Trash2, UserCheck, UserX, Shield } from 'lucide-react'
import { useForm } from 'react-hook-form'
import Card from '../../components/ui/Card'
import Button from '../../components/ui/Button'
import Input from '../../components/ui/Input'
import Modal from '../../components/ui/Modal'
import Badge from '../../components/ui/Badge'
import { SkeletonTable } from '../../components/ui/Skeleton'
import { adminApi } from '../../api/index'
import { QUERY_KEYS } from '../../constants/queryKeys'
import { formatCurrency, formatDate } from '../../utils/formatters'
import toast from 'react-hot-toast'

export default function AdminUsers() {
  const { t } = useTranslation()
  const qc = useQueryClient()
  const [search, setSearch] = useState('')
  const [createModal, setCreateModal] = useState(false)

  const { data, isLoading } = useQuery({
    queryKey: [...QUERY_KEYS.ADMIN_USERS, search],
    queryFn: () => adminApi.getUsers({ search, pageSize: 50 }),
    select: res => res.data.data,
  })

  const toggleMutation = useMutation({
    mutationFn: adminApi.toggleSuspend,
    onSuccess: () => qc.invalidateQueries({ queryKey: QUERY_KEYS.ADMIN_USERS }),
  })
  const roleMutation = useMutation({
    mutationFn: ({ id, role }) => adminApi.changeRole(id, role),
    onSuccess: () => { qc.invalidateQueries({ queryKey: QUERY_KEYS.ADMIN_USERS }); toast.success(t('admin.roleUpdated')) },
  })
  const deleteMutation = useMutation({
    mutationFn: adminApi.deleteUser,
    onSuccess: () => { qc.invalidateQueries({ queryKey: QUERY_KEYS.ADMIN_USERS }); toast.success(t('admin.userDeleted')) },
  })
  const createMutation = useMutation({
    mutationFn: adminApi.createUser,
    onSuccess: () => { qc.invalidateQueries({ queryKey: QUERY_KEYS.ADMIN_USERS }); setCreateModal(false); toast.success(t('admin.userCreated')) },
  })

  const users = data?.items || []

  return (
    <div>
      <div className="flex flex-col sm:flex-row gap-3 mb-4">
        <div className="relative flex-1">
          <Search size={15} className="absolute start-3 top-1/2 -translate-y-1/2 text-gray-400" />
          <input className="input ps-9 text-sm" placeholder={t('admin.searchUsers')} value={search} onChange={e => setSearch(e.target.value)} />
        </div>
        <Button variant="primary" size="sm" icon={Plus} onClick={() => setCreateModal(true)}>{t('admin.addUser')}</Button>
      </div>

      <Card padding={false}>
        {isLoading ? <div className="p-4"><SkeletonTable rows={5} /></div> : (
          <div className="overflow-x-auto">
            <table className="table">
              <thead><tr>
                <th>User</th>
                <th>{t('admin.role')}</th>
                <th>{t('admin.totalExpenses')}</th>
                <th>{t('admin.joined')}</th>
                <th className="text-end">{t('common.actions')}</th>
              </tr></thead>
              <tbody>
                {users.map(u => (
                  <tr key={u.id}>
                    <td>
                      <div className="flex items-center gap-2">
                        <div className="w-8 h-8 rounded-full bg-primary-50 flex items-center justify-center text-xs font-bold text-primary-600 flex-shrink-0">
                          {u.fullName?.slice(0,2).toUpperCase()}
                        </div>
                        <div className="min-w-0">
                          <p className="text-sm font-medium text-gray-900 truncate">{u.fullName}</p>
                          <p className="text-xs text-gray-400 truncate">{u.email}</p>
                        </div>
                      </div>
                    </td>
                    <td><Badge variant={u.role==='Admin' ? 'blue' : 'green'}>{u.role}</Badge></td>
                    <td>
                      <p className="text-sm">{u.totalExpenses}</p>
                      <p className="text-xs text-gray-400">{formatCurrency(u.totalAmount)}</p>
                    </td>
                    <td className="text-sm text-gray-500 whitespace-nowrap">{formatDate(u.createdAt)}</td>
                    <td>
                      <div className="flex items-center justify-end gap-1">
                        <button onClick={() => roleMutation.mutate({ id: u.id, role: u.role==='Admin' ? 'User' : 'Admin' })}
                          title={u.role==='Admin' ? t('admin.removeAdmin') : t('admin.makeAdmin')}
                          className={`p-1.5 rounded-lg ${u.role==='Admin' ? 'text-info-400 hover:bg-info-50' : 'text-gray-400 hover:bg-gray-100'}`}>
                          <Shield size={14} />
                        </button>
                        <button onClick={() => toggleMutation.mutate(u.id)}
                          title={u.isSuspended ? t('admin.unsuspend') : t('admin.suspend')}
                          className={`p-1.5 rounded-lg ${u.isSuspended ? 'text-primary-600 hover:bg-primary-50' : 'text-gray-400 hover:bg-gray-100'}`}>
                          {u.isSuspended ? <UserCheck size={14} /> : <UserX size={14} />}
                        </button>
                        <button onClick={() => { if(confirm(`Delete ${u.fullName}?`)) deleteMutation.mutate(u.id) }}
                          className="p-1.5 text-gray-400 hover:text-danger-400 hover:bg-danger-50 rounded-lg"><Trash2 size={14} /></button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </Card>

      <Modal open={createModal} onClose={() => setCreateModal(false)} title={t('admin.createUser')} size="sm"
        footer={<><Button variant="secondary" onClick={() => setCreateModal(false)}>{t('common.cancel')}</Button>
          <Button variant="primary" loading={createMutation.isPending} onClick={() => document.getElementById('create-user-submit').click()}>{t('common.create')}</Button></>}>
        <CreateUserForm t={t} onSubmit={(data) => createMutation.mutate(data)} />
      </Modal>
    </div>
  )
}

function CreateUserForm({ t, onSubmit }) {
  const { register, handleSubmit, formState: { errors } } = useForm({ defaultValues: { role: 'User' } })
  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <Input label={t('auth.fullName')} error={errors.fullName?.message} {...register('fullName', { required: true })} />
      <Input label={t('auth.email')} type="email" error={errors.email?.message} {...register('email', { required: true })} />
      <Input label={t('auth.password')} type="password" error={errors.password?.message}
        {...register('password', { required: true, minLength: { value: 6, message: t('auth.validation.passwordMin') } })} />
      <div><label className="label">{t('admin.role')}</label>
        <select className="input" {...register('role')}><option value="User">User</option><option value="Admin">Admin</option></select></div>
      <button type="submit" id="create-user-submit" className="hidden" />
    </form>
  )
}
