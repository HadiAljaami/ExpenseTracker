import { Routes, Route, NavLink } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { BarChart2, Users, Receipt, Activity } from 'lucide-react'
import { clsx } from 'clsx'
import AdminStats from './AdminStats'
import AdminUsers from './AdminUsers'
import AdminExpenses from './AdminExpenses'
import AdminAuditLog from './AdminAuditLog'
import PageLayout from '../../components/layout/PageLayout'

export default function AdminPage() {
  const { t } = useTranslation()
  const tabs = [
    { to: '', label: t('admin.overview'), icon: BarChart2, end: true },
    { to: 'users', label: t('admin.users'), icon: Users },
    { to: 'expenses', label: t('admin.expenses'), icon: Receipt },
    { to: 'audit', label: t('admin.auditLog'), icon: Activity },
  ]
  return (
    <PageLayout title={t('admin.title')}>
      <div className="flex gap-1 overflow-x-auto scrollbar-hide mb-5 border-b border-gray-100">
        {tabs.map(tab => (
          <NavLink key={tab.to} to={tab.to} end={tab.end}
            className={({ isActive }) => clsx(
              'flex items-center gap-2 px-4 py-2.5 text-sm font-medium whitespace-nowrap border-b-2 -mb-px transition-colors',
              isActive ? 'border-primary-400 text-primary-600' : 'border-transparent text-gray-500 hover:text-gray-800'
            )}>
            <tab.icon size={15} />{tab.label}
          </NavLink>
        ))}
      </div>
      <Routes>
        <Route index element={<AdminStats />} />
        <Route path="users" element={<AdminUsers />} />
        <Route path="expenses" element={<AdminExpenses />} />
        <Route path="audit" element={<AdminAuditLog />} />
      </Routes>
    </PageLayout>
  )
}
