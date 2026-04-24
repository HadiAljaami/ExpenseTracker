import { TrendingUp, TrendingDown, Minus } from 'lucide-react'
import { clsx } from 'clsx'

export default function StatCard({
  label,
  value,
  change,
  changeLabel,
  icon: Icon,
  iconColor = 'text-primary-400',
  iconBg = 'bg-primary-50',
}) {
  const isPositive = change > 0
  const isNeutral = change === 0 || change === undefined

  return (
    <div className="stat-card">
      <div className="flex items-center justify-between gap-3">
        {/* Text */}
        <div className="flex-1 min-w-0">
          <p className="text-xs font-medium text-gray-500 truncate">{label}</p>
          <p className="mt-1 text-xl font-semibold text-gray-900 truncate leading-tight">{value}</p>
          {change !== undefined && (
            <div className={clsx(
              'mt-1.5 inline-flex items-center gap-1 text-xs font-medium',
              isNeutral ? 'text-gray-500' : isPositive ? 'text-primary-600' : 'text-danger-400'
            )}>
              {isNeutral
                ? <Minus size={11} />
                : isPositive
                  ? <TrendingUp size={11} />
                  : <TrendingDown size={11} />
              }
              <span className="truncate">
                {isNeutral
                  ? changeLabel
                  : `${isPositive ? '+' : ''}${Number(change).toFixed(1)}% ${changeLabel || ''}`
                }
              </span>
            </div>
          )}
        </div>

        {/* Icon — perfectly centered */}
        {Icon && (
          <div className={clsx(
            'w-11 h-11 rounded-xl flex items-center justify-center flex-shrink-0',
            iconBg
          )}>
            <Icon size={20} className={iconColor} strokeWidth={1.8} />
          </div>
        )}
      </div>
    </div>
  )
}
