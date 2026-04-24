import { useTranslation } from 'react-i18next'
import { useQuery } from '@tanstack/react-query'
import { Activity } from 'lucide-react'
import Card from '../../components/ui/Card'
import Badge from '../../components/ui/Badge'
import EmptyState from '../../components/ui/EmptyState'
import { adminApi } from '../../api/index'
import { QUERY_KEYS } from '../../constants/queryKeys'
import { formatDateTime } from '../../utils/formatters'

const ACTION_BADGE = {
  DeleteUser: 'red', ChangeRole: 'blue', SuspendUser: 'amber',
  UnsuspendUser: 'green', DeleteExpense: 'red',
  CreateCategory: 'green', UpdateCategory: 'blue', DeleteCategory: 'red',
}

export default function AdminAuditLog() {
  const { t } = useTranslation()
  const { data, isLoading } = useQuery({
    queryKey: QUERY_KEYS.AUDIT_LOGS,
    queryFn: () => adminApi.getAuditLogs({ pageSize: 50 }),
    select: res => res.data.data,
  })
  const logs = data?.items || []

  return (
    <div>
      {isLoading ? (
        <div className="space-y-2">{[...Array(5)].map((_,i) => <div key={i} className="skeleton h-14 rounded-lg" />)}</div>
      ) : logs.length === 0 ? (
        <Card><EmptyState icon={Activity} title={t('admin.noAuditLogs')} description={t('admin.noAuditLogsDesc')} /></Card>
      ) : (
        <Card padding={false}>
          <div className="overflow-x-auto">
            <table className="table">
              <thead><tr>
                <th>Admin</th>
                <th>Action</th>
                <th>Entity</th>
                <th>Details</th>
                <th>IP</th>
                <th>Time</th>
              </tr></thead>
              <tbody>
                {logs.map(log => (
                  <tr key={log.id}>
                    <td className="text-sm text-gray-700">{log.adminEmail}</td>
                    <td><Badge variant={ACTION_BADGE[log.action]||'gray'}>{log.action}</Badge></td>
                    <td className="text-sm text-gray-500">{log.entityType} #{log.entityId}</td>
                    <td className="text-xs text-gray-400 max-w-[200px] truncate">{log.oldValue||log.newValue||'—'}</td>
                    <td className="text-xs text-gray-400">{log.ipAddress||'—'}</td>
                    <td className="text-xs text-gray-400 whitespace-nowrap">{formatDateTime(log.createdAt)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </Card>
      )}
    </div>
  )
}
