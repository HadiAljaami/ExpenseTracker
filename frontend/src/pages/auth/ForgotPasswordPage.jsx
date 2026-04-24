import { useForm } from 'react-hook-form'
import { Link } from 'react-router-dom'
import { Mail } from 'lucide-react'
import { useMutation } from '@tanstack/react-query'
import { authApi } from '../../api/auth.api'
import Input from '../../components/ui/Input'
import Button from '../../components/ui/Button'
import toast from 'react-hot-toast'

export default function ForgotPasswordPage() {
  const { register, handleSubmit, formState: { errors } } = useForm()

  const mutation = useMutation({
    mutationFn: ({ email }) => authApi.forgotPassword(email),
    onSuccess: () => toast.success('Reset link sent! Check your email.'),
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
            <h2 className="text-xl font-bold text-gray-900">Forgot password?</h2>
            <p className="text-sm text-gray-500 mt-1">Enter your email and we'll send a reset link.</p>
          </div>
          <form onSubmit={handleSubmit((d) => mutation.mutate(d))} className="space-y-4">
            <Input
              label="Email address"
              type="email"
              icon={Mail}
              placeholder="you@example.com"
              error={errors.email?.message}
              {...register('email', { required: 'Email is required' })}
            />
            <Button variant="primary" className="w-full" loading={mutation.isPending} type="submit">
              Send reset link
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
