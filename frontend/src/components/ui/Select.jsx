import { forwardRef } from 'react'
import { clsx } from 'clsx'

const Select = forwardRef(({ label, error, options = [], placeholder, className, ...props }, ref) => {
  return (
    <div className="w-full">
      {label && <label className="label">{label}</label>}
      <select
        ref={ref}
        className={clsx('input', error && 'input-error', className)}
        {...props}
      >
        {placeholder && <option value="">{placeholder}</option>}
        {options.map((opt) => (
          <option key={opt.value} value={opt.value}>
            {opt.label}
          </option>
        ))}
      </select>
      {error && <p className="error-text">{error}</p>}
    </div>
  )
})

Select.displayName = 'Select'
export default Select
