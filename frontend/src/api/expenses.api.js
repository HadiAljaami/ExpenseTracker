import api from './axios'

export const expensesApi = {
  getAll: (params) => api.get('/expenses', { params }),
  getById: (id) => api.get(`/expenses/${id}`),
  create: (data) => api.post('/expenses', data),
  update: (id, data) => api.put(`/expenses/${id}`, data),
  delete: (id) => api.delete(`/expenses/${id}`),
  getDeleted: () => api.get('/expenses/deleted'),
  restore: (id) => api.patch(`/expenses/${id}/restore`),
}
