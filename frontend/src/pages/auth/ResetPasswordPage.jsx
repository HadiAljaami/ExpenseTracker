import { useForm } from 'react-hook-form'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { Lock } from 'lucide-react'
import { useMutation } from '@tanstack/react-query'
import { authApi } from '../../api/auth.api'
import Input from '../../components/ui/Input'
import Button from '../../components/ui/Button'
import toast from 'react-hot-toast'

export default function ResetPasswordPage() {
  const [params] = useSearchParams()
  const navigate = useNavigate()
  const { register, handleSubmit, watch, formState: { errors } } = useForm()
  const password = watch('newPassword')

  const mutation = useMutation({
    mutationFn: (data) => authApi.resetPassword(data),
    onSuccess: () => {
      toast.success('Password reset successfully!')
      navigate('/login')
    },
  })

  const onSubmit = (data) => mutation.mutate({
    email: params.get('email') || data.email,
    token: params.get('token') || data.token,
    newPassword: data.newPassword,
    confirmNewPassword: data.confirmNewPassword,
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
          <div className="mb-6">
            <h2 className="text-xl font-bold text-gray-900">Reset password</h2>
            <p className="text-sm text-gray-500 mt-1">Enter your new password below.</p>
          </div>
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <Input
              label="New password"
              type="password"
              icon={Lock}
              placeholder="Min. 6 characters"
              error={errors.newPassword?.message}
              {...register('newPassword', {
                required: 'Password is required',
                minLength: { value: 6, message: 'At least 6 characters' },
                pattern: { value: /^(?=.*[A-Z])(?=.*[0-9])/, message: 'Must contain uppercase and number' }
              })}
            />
            <Input
              label="Confirm new password"
              type="password"
              icon={Lock}
              placeholder="Repeat password"
              error={errors.confirmNewPassword?.message}
              {...register('confirmNewPassword', {
                required: 'Please confirm',
                validate: (v) => v === password || 'Passwords do not match'
              })}
            />
            <Button variant="primary" className="w-full" loading={mutation.isPending} type="submit">
              Reset password
            </Button>
          </form>
          <p className="text-center text-sm text-gray-500 mt-6">
            <Link to="/login" className="text-primary-600 font-medium hover:underline">Back to login</Link>
          </p>
        </div>
      </div>
    </div>
  )
}
