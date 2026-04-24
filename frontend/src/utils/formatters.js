// ── Currency ──────────────────────────────────────────────────────────────────
export const formatCurrency = (amount, currency = 'SAR') => {
  return new Intl.NumberFormat('en-SA', {
    style: 'currency',
    currency,
    minimumFractionDigits: 0,
    maximumFractionDigits: 2,
  }).format(amount)
}

// ── Date ──────────────────────────────────────────────────────────────────────
export const formatDate = (date, options = {}) => {
  const defaultOptions = { year: 'numeric', month: 'short', day: 'numeric' }
  return new Intl.DateTimeFormat('en-SA', { ...defaultOptions, ...options }).format(new Date(date))
}

export const formatDateShort = (date) =>
  new Intl.DateTimeFormat('en-SA', { month: 'short', day: 'numeric' }).format(new Date(date))

export const formatDateTime = (date) =>
  new Intl.DateTimeFormat('en-SA', {
    year: 'numeric', month: 'short', day: 'numeric',
    hour: '2-digit', minute: '2-digit',
  }).format(new Date(date))

export const formatRelativeTime = (date) => {
  const now = new Date()
  const diff = now - new Date(date)
  const seconds = Math.floor(diff / 1000)
  const minutes = Math.floor(seconds / 60)
  const hours = Math.floor(minutes / 60)
  const days = Math.floor(hours / 24)

  if (days > 7) return formatDate(date)
  if (days > 0) return `${days}d ago`
  if (hours > 0) return `${hours}h ago`
  if (minutes > 0) return `${minutes}m ago`
  return 'just now'
}

// ── Numbers ───────────────────────────────────────────────────────────────────
export const formatNumber = (num) =>
  new Intl.NumberFormat('en-SA').format(num)

export const formatPercentage = (value, decimals = 1) =>
  `${Number(value).toFixed(decimals)}%`

export const formatCompact = (num) =>
  new Intl.NumberFormat('en-SA', { notation: 'compact', maximumFractionDigits: 1 }).format(num)

// ── Misc ──────────────────────────────────────────────────────────────────────
export const getMonthName = (month, year) =>
  new Date(year, month - 1, 1).toLocaleString('en-SA', { month: 'long', year: 'numeric' })

export const getCurrentMonthYear = () => {
  const now = new Date()
  return { month: now.getMonth() + 1, year: now.getFullYear() }
}

export const truncate = (str, length = 30) =>
  str?.length > length ? `${str.slice(0, length)}...` : str
