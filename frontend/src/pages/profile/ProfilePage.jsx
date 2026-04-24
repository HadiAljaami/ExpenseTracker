import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { useForm } from 'react-hook-form'
import { User, Lock, Trash2, Save } from 'lucide-react'
import PageLayout from '../../components/layout/PageLayout'
import Card, { CardHeader } from '../../components/ui/Card'
import Button from '../../components/ui/Button'
import Input from '../../components/ui/Input'
import Modal from '../../components/ui/Modal'
import Badge from '../../components/ui/Badge'
import { usersApi } from '../../api/index'
import { QUERY_KEYS } from '../../constants/queryKeys'
import { useAuthStore } from '../../store/authStore'
import { useNavigate } from 'react-router-dom'
import toast from 'react-hot-toast'

const CURRENCIES = [
  { value: 'SAR', label: 'SAR — ريال سعودي' },
  { value: 'USD', label: 'USD — US Dollar' },
  { value: 'EUR', label: 'EUR — Euro' },
  { value: 'GBP', label: 'GBP — British Pound' },
  { value: 'AED', label: 'AED — درهم إماراتي' },
]

export default function ProfilePage() {
  const { user, updateUser, logout } = useAuthStore()
  const { t } = useTranslation()
  const qc = useQueryClient()
  const navigate = useNavigate()
  const [deleteConfirm, setDeleteConfirm] = useState(false)

  const { data: profile } = useQuery({
    queryKey: QUERY_KEYS.PROFILE,
    queryFn: () => usersApi.getProfile(),
    select: res => res.data.data,
  })

  const profileForm = useForm({ defaultValues: { fullName: user?.fullName, currency: user?.currency || 'SAR' } })
  const pwForm = useForm()

  const updateMutation = useMutation({
    mutationFn: usersApi.updateProfile,
    onSuccess: (res) => { updateUser(res.data.data); qc.invalidateQueries({ queryKey: QUERY_KEYS.PROFILE }); toast.success(t('profile.updated')) },
  })
  const pwMutation = useMutation({
    mutationFn: usersApi.changePassword,
    onSuccess: () => { pwForm.reset(); toast.success(t('profile.passwordChanged')) },
    onError: (err) => toast.error(err.response?.data?.message || 'Failed'),
  })
  const deleteMutation = useMutation({
    mutationFn: usersApi.deleteAccount,
    onSuccess: () => { logout(); navigate('/login'); toast.success(t('profile.accountDeleted')) },
  })

  return (
    <PageLayout title={t('profile.title')}>
      <div className="max-w-2xl space-y-4">
        <Card>
          <div className="flex items-center gap-4">
            <div className="w-16 h-16 rounded-full bg-primary-50 flex items-center justify-center text-primary-600 text-2xl font-bold flex-shrink-0">
              {user?.fullName?.slice(0, 2).toUpperCase()}
            </div>
            <div>
              <p className="text-base font-semibold text-gray-900">{profile?.fullName || user?.fullName}</p>
              <p className="text-sm text-gray-500">{profile?.email || user?.email}</p>
              <Badge variant={user?.role === 'Admin' ? 'blue' : 'green'} className="mt-1">{user?.role}</Badge>
            </div>
          </div>
        </Card>

        <Card>
          <CardHeader title={t('profile.editProfile')} />
          <form onSubmit={profileForm.handleSubmit((data) => updateMutation.mutate(data))} className="space-y-4">
            <Input label={t('auth.fullName')} icon={User}
              error={profileForm.formState.errors.fullName?.message}
              {...profileForm.register('fullName', { required: t('auth.validation.nameRequired') })} />
            <div><label className="label">{t('profile.currency')}</label>
              <select className="input" {...profileForm.register('currency')}>
                {CURRENCIES.map(c => <option key={c.value} value={c.value}>{c.label}</option>)}
              </select>
            </div>
            <div className="flex justify-end">
              <Button variant="primary" size="sm" icon={Save} loading={updateMutation.isPending} type="submit">{t('common.save')}</Button>
            </div>
          </form>
        </Card>

        <Card>
          <CardHeader title={t('profile.changePassword')} />
          <form onSubmit={pwForm.handleSubmit((data) => pwMutation.mutate(data))} className="space-y-4">
            <Input label={t('profile.currentPassword')} type="password" icon={Lock}
              error={pwForm.formState.errors.currentPassword?.message}
              {...pwForm.register('currentPassword', { required: t('auth.validation.passwordRequired') })} />
            <Input label={t('profile.newPassword')} type="password" icon={Lock}
              error={pwForm.formState.errors.newPassword?.message}
              {...pwForm.register('newPassword', { required: t('auth.validation.passwordRequired'), minLength: { value: 6, message: t('auth.validation.passwordMin') }, pattern: { value: /^(?=.*[A-Z])(?=.*[0-9])/, message: t('auth.validation.passwordPattern') } })} />
            <Input label={t('profile.confirmNew')} type="password" icon={Lock}
              error={pwForm.formState.errors.confirmNewPassword?.message}
              {...pwForm.register('confirmNewPassword', { required: t('auth.validation.confirmRequired'), validate: v => v === pwForm.watch('newPassword') || t('auth.validation.passwordsMatch') })} />
            <div className="flex justify-end">
              <Button variant="primary" size="sm" icon={Lock} loading={pwMutation.isPending} type="submit">{t('profile.changePassword')}</Button>
            </div>
          </form>
        </Card>

        <Card className="border-danger-400 border">
          <CardHeader title={t('profile.dangerZone')} />
          <p className="text-sm text-gray-500 mb-3">{t('profile.dangerDesc')}</p>
          <Button variant="danger" size="sm" icon={Trash2} onClick={() => setDeleteConfirm(true)}>{t('profile.deleteAccount')}</Button>
        </Card>
      </div>

      <Modal open={deleteConfirm} onClose={() => setDeleteConfirm(false)} title={t('profile.deleteAccount')} size="sm"
        footer={<><Button variant="secondary" onClick={() => setDeleteConfirm(false)}>{t('common.cancel')}</Button>
          <Button variant="danger" loading={deleteMutation.isPending} onClick={() => deleteMutation.mutate()}>{t('profile.yesDelete')}</Button></>}>
        <p className="text-sm text-gray-600" dangerouslySetInnerHTML={{ __html: t('profile.deleteConfirm') }} />
      </Modal>
    </PageLayout>
  )
}
