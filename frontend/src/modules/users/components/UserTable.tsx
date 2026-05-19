import { Badge } from '@/shared/components/ui/badge'
import { Button } from '@/shared/components/ui/button'
import type { UserSummary } from '../types'
import { MoreVertical } from 'lucide-react'

interface UserTableProps {
  users: UserSummary[]
  onEdit: (userId: string) => void
  isLoading?: boolean
}

const SKELETON_KEYS = ['sk-a', 'sk-b', 'sk-c', 'sk-d', 'sk-e']

export function UserTable({ users, onEdit, isLoading }: Readonly<UserTableProps>) {
  if (isLoading) {
    return (
      <div className="space-y-2">
        {SKELETON_KEYS.map((k) => (
          <div key={k} className="h-12 bg-stone-100 rounded" />
        ))}
      </div>
    )
  }

  if (users.length === 0) {
    return (
      <div className="text-center py-12">
        <p className="text-stone-500">No hay usuarios registrados</p>
      </div>
    )
  }

  return (
    <div className="border border-stone-200 rounded-lg overflow-hidden">
      <table className="w-full">
        <thead className="bg-stone-50 border-b border-stone-200">
          <tr>
            <th className="px-6 py-3 text-left text-sm font-medium text-stone-600">Nombre</th>
            <th className="px-6 py-3 text-left text-sm font-medium text-stone-600">Email</th>
            <th className="px-6 py-3 text-left text-sm font-medium text-stone-600">Rol</th>
            <th className="px-6 py-3 text-left text-sm font-medium text-stone-600">Estado</th>
            <th className="px-6 py-3 text-right text-sm font-medium text-stone-600">Acciones</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-stone-200">
          {users.map((user) => (
            <tr key={user.id} className="hover:bg-stone-50">
              <td className="px-6 py-4 text-sm text-stone-900">{user.fullName}</td>
              <td className="px-6 py-4 text-sm text-stone-600">{user.email}</td>
              <td className="px-6 py-4 text-sm">
                <Badge variant="outline">{user.role}</Badge>
              </td>
              <td className="px-6 py-4 text-sm">
                {user.isActive ? (
                  <Badge className="bg-emerald-50 text-emerald-700 border-emerald-200">Activo</Badge>
                ) : (
                  <Badge className="bg-stone-100 text-stone-600 border-stone-200">Inactivo</Badge>
                )}
              </td>
              <td className="px-6 py-4 text-right">
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => onEdit(user.id)}
                >
                  <MoreVertical className="h-4 w-4" />
                </Button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
