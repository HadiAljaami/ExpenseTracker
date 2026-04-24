import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { Link, useNavigate } from 'react-router-dom'
import { Mail, Lock, User, Eye, EyeOff } from 'lucide-react'
import { useMutation } from '@tanstack/react-query'
import { authApi } from '../../api/auth.api'
import { useAuthStore } from '../../store/authStore'
import Input from '../../components/ui/Input'
import Button from '../../components/ui/Button'
import toast from 'react-hot-toast'

export default function RegisterPage() {
  const [showPassword, setShowPassword] = useState(false)
  const { setAuth } = useAuthStore()
  const navigate = useNavigate()

  const { register, handleSubmit, watch, formState: { errors } } = useForm()
  const password = watch('password')

  const registerMutation = useMutation({
    mutationFn: authApi.register,
    onSuccess: ({ data }) => {
      const { accessToken, refreshToken, fullName, email, role, currency } = data.data
      setAuth({ fullName, email, role, currency }, accessToken, refreshToken)
      toast.success(`Account created! Welcome, ${fullName}!`)
      navigate('/dashboard')
    },
  })

  const onSubmit = ({ confirmPassword, ...data }) => registerMutation.mutate(data)

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
            <h2 className="text-xl font-bold text-gray-900">Create account</h2>
            <p className="text-sm text-gray-500 mt-1">Start tracking your expenses today</p>
          </div>

          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <Input
              label="Full name"
              type="text"
              icon={User}
              placeholder="Hadi Ahmed"
              error={errors.fullName?.message}
              {...register('fullName', {
                required: 'Full name is required',
                minLength: { value: 2, message: 'At least 2 characters' }
              })}
            />

            <Input
              label="Email address"
              type="email"
              icon={Mail}
              placeholder="you@example.com"
              error={errors.email?.message}
              {...register('email', {
                required: 'Email is required',
                pattern: { value: /^\S+@\S+\.\S+$/, message: 'Invalid email' }
              })}
            />

            <div className="relative">
              <Input
                label="Password"
                type={showPassword ? 'text' : 'password'}
                icon={Lock}
                placeholder="Min. 6 characters"
                error={errors.password?.message}
                className="pr-10"
                {...register('password', {
                  required: 'Password is required',
                  minLength: { value: 6, message: 'At least 6 characters' },
                  pattern: {
                    value: /^(?=.*[A-Z])(?=.*[0-9])/,
                    message: 'Must contain uppercase letter and number'
                  }
                })}
              />
              <button type="button" onClick={() => setShowPassword(!showPassword)}
                className="absolute right-3 top-8 text-gray-400 hover:text-gray-600">
                {showPassword ? <EyeOff size={16} /> : <Eye size={16} />}
              </button>
            </div>

            <Input
              label="Confirm password"
              type="password"
              icon={Lock}
              placeholder="Repeat password"
              error={errors.confirmPassword?.message}
              {...register('confirmPassword', {
                required: 'Please confirm your password',
                validate: (v) => v === password || 'Passwords do not match'
              })}
            />

            <Button
              variant="primary"
              className="w-full"
              loading={registerMutation.isPending}
              type="submit"
            >
              Create account
            </Button>
          </form>

          <p className="text-center text-sm text-gray-500 mt-6">
            Already have an account?{' '}
            <Link to="/login" className="text-primary-600 font-medium hover:underline">
              Sign in
            </Link>
          </p>
        </div>
      </div>
    </div>
  )
}
