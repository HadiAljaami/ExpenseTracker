import api from './axios'

export { authApi } from './auth.api'
export { expensesApi } from './expenses.api'
export { budgetsApi } from './budgets.api'

export const categoriesApi = {
  getAll: () => api.get('/categories'),
  create: (data) => api.post('/categories', data),
  update: (id, data) => api.put(`/categories/${id}`, data),
  delete: (id) => api.delete(`/categories/${id}`),
}

export const insightsApi = {
  getSummary: (params) => api.get('/insights', { params }),
}

export const reportsApi = {
  getMonthly: (params) => api.get('/reports/monthly', { params }),
  getYearly: (params) => api.get('/reports/yearly', { params }),
  exportExcel: (params) => api.get('/reports/export/excel', { params, responseType: 'blob' }),
}

export const alertsApi = {
  getAll: (params) => api.get('/alerts', { params }),
  markAsRead: (id) => api.patch(`/alerts/${id}/read`),
  markAllAsRead: () => api.patch('/alerts/read-all'),
}

export const usersApi = {
  getProfile: () => api.get('/users/me'),
  updateProfile: (data) => api.put('/users/me', data),
  changePassword: (data) => api.put('/users/me/change-password', data),
  deleteAccount: () => api.delete('/users/me'),
}

export const recurringApi = {
  getAll: () => api.get('/recurring-expenses'),
  create: (data) => api.post('/recurring-expenses', data),
  toggle: (id) => api.patch(`/recurring-expenses/${id}/toggle`),
  delete: (id) => api.delete(`/recurring-expenses/${id}`),
}

export const adminApi = {
  getUsers: (params) => api.get('/admin/users', { params }),
  getUserById: (id) => api.get(`/admin/users/${id}`),
  createUser: (data) => api.post('/admin/users', data),
  changeRole: (id, role) => api.patch(`/admin/users/${id}/role`, { role }),
  toggleSuspend: (id) => api.patch(`/admin/users/${id}/suspend`),
  deleteUser: (id) => api.delete(`/admin/users/${id}`),
  getExpenses: (params) => api.get('/admin/expenses', { params }),
  deleteExpense: (id) => api.delete(`/admin/expenses/${id}`),
  getStats: () => api.get('/admin/stats'),
  getMonthlyGrowth: (params) => api.get('/admin/stats/monthly', { params }),
  getSystemReport: (params) => api.get('/admin/reports', { params }),
  getAuditLogs: (params) => api.get('/admin/audit-logs', { params }),
}
