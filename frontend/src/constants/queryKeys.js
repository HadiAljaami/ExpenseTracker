export const QUERY_KEYS = {
  // Auth
  PROFILE: ['profile'],

  // Expenses
  EXPENSES: ['expenses'],
  EXPENSE: (id) => ['expenses', id],
  DELETED_EXPENSES: ['expenses', 'deleted'],

  // Budgets
  BUDGETS: (month, year) => ['budgets', month, year],

  // Categories
  CATEGORIES: ['categories'],

  // Insights
  INSIGHTS: (month, year) => ['insights', month, year],

  // Reports
  MONTHLY_REPORT: (month, year) => ['reports', 'monthly', month, year],
  YEARLY_REPORT: (year) => ['reports', 'yearly', year],

  // Alerts
  ALERTS: ['alerts'],

  // Recurring
  RECURRING: ['recurring'],

  // Admin
  ADMIN_USERS: ['admin', 'users'],
  ADMIN_USER: (id) => ['admin', 'users', id],
  ADMIN_EXPENSES: ['admin', 'expenses'],
  ADMIN_STATS: ['admin', 'stats'],
  ADMIN_MONTHLY: (year) => ['admin', 'stats', 'monthly', year],
  ADMIN_REPORT: (month, year) => ['admin', 'reports', month, year],
  AUDIT_LOGS: ['admin', 'audit-logs'],
}
