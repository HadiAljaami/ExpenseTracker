import { forwardRef } from 'react'
import { clsx } from 'clsx'

const Input = forwardRef(({ label, error, icon: Icon, className, ...props }, ref) => {
  return (
    <div className="w-full">
      {label && <label className="label">{label}</label>}
      <div className="relative">
        {Icon && (
          <div className="absolute inset-y-0 left-3 flex items-center pointer-events-none text-gray-400">
            <Icon size={16} />
          </div>
        )}
        <input
          ref={ref}
          className={clsx(
            'input',
            Icon && 'pl-9',
            error && 'input-error',
            className
          )}
          {...props}
        />
      </div>
      {error && <p className="error-text">{error}</p>}
    </div>
  )
})

Input.displayName = 'Input'
export default Input
