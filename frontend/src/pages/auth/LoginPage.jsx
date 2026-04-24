import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { Link, useNavigate } from 'react-router-dom'
import { Mail, Lock, Eye, EyeOff } from 'lucide-react'
import { useMutation } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { authApi } from '../../api/auth.api'
import { useAuthStore } from '../../store/authStore'
import Input from '../../components/ui/Input'
import Button from '../../components/ui/Button'
import LangSwitcher from '../../components/ui/LangSwitcher'
import toast from 'react-hot-toast'

export default function LoginPage() {
  const [showPassword, setShowPassword] = useState(false)
  const { setAuth } = useAuthStore()
  const navigate = useNavigate()
  const { t } = useTranslation()

  const { register, handleSubmit, formState: { errors } } = useForm()

  const loginMutation = useMutation({
    mutationFn: (data) => authApi.login(data),
    onSuccess: ({ data }) => {
      const { accessToken, refreshToken, fullName, email, role, currency } = data.data
      setAuth({ fullName, email, role, currency }, accessToken, refreshToken)
      toast.success(t('auth.loginSuccess', { name: fullName }))
      navigate('/dashboard')
    },
    onError: (err) => toast.error(err.response?.data?.message || t('auth.validation.emailInvalid')),
  })

  return (
    <div className="min-h-screen bg-gradient-to-br from-primary-50 to-white flex items-center justify-center p-4">
      <div className="w-full max-w-md">
        <div className="flex items-center justify-center gap-3 mb-8">
          <div className="w-10 h-10 bg-primary-400 rounded-xl flex items-center justify-center">
            <span className="text-white font-bold">ET</span>
          </div>
          <span className="text-xl font-bold text-gray-900">Expense Tracker</span>
        </div>

        <div className="card p-8">
          <div className="flex items-start justify-between mb-6">
            <div>
              <h2 className="text-xl font-bold text-gray-900">{t('auth.welcomeBack')}</h2>
              <p className="text-sm text-gray-500 mt-1">{t('auth.signInDesc')}</p>
            </div>
            <LangSwitcher />
          </div>

          <form onSubmit={handleSubmit((d) => loginMutation.mutate(d))} className="space-y-4">
            <Input label={t('auth.email')} type="email" icon={Mail} placeholder="you@example.com"
              error={errors.email?.message}
              {...register('email', { required: t('auth.validation.emailRequired'), pattern: { value: /^\S+@\S+\.\S+$/, message: t('auth.validation.emailInvalid') } })} />

            <div className="relative">
              <Input label={t('auth.password')} type={showPassword ? 'text' : 'password'} icon={Lock}
                placeholder="••••••••" error={errors.password?.message} className="pr-10"
                {...register('password', { required: t('auth.validation.passwordRequired') })} />
              <button type="button" onClick={() => setShowPassword(!showPassword)}
                className="absolute end-3 top-8 text-gray-400 hover:text-gray-600">
                {showPassword ? <EyeOff size={16} /> : <Eye size={16} />}
              </button>
            </div>

            <div className="flex justify-end">
              <Link to="/forgot-password" className="text-xs text-primary-600 hover:underline">{t('auth.forgotPassword')}</Link>
            </div>

            <Button variant="primary" className="w-full" loading={loginMutation.isPending} type="submit">
              {t('auth.signIn')}
            </Button>
          </form>

          <p className="text-center text-sm text-gray-500 mt-6">
            {t('auth.noAccount')}{' '}
            <Link to="/register" className="text-primary-600 font-medium hover:underline">{t('auth.createOne')}</Link>
          </p>
        </div>
      </div>
    </div>
  )
}
