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
      <div className="space-y-2 p-5">
        {SKELETON_KEYS.map((k) => (
          <div key={k} className="h-12 rounded bg-stone-100" />
        ))}
      </div>
    )
  }

  if (users.length === 0) {
    return (
      <div className="px-5 py-12 text-center">
        <p className="text-sm font-medium text-stone-500">No hay usuarios registrados.</p>
      </div>
    )
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full text-sm">
        <thead className="border-b border-stone-200 bg-stone-50">
          <tr>
            <th className="px-5 py-3 text-left font-medium text-stone-600">Nombre</th>
            <th className="px-5 py-3 text-left font-medium text-stone-600">Email</th>
            <th className="px-5 py-3 text-left font-medium text-stone-600">Rol</th>
            <th className="px-5 py-3 text-left font-medium text-stone-600">Estado</th>
            <th className="px-5 py-3 text-right font-medium text-stone-600">Acciones</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-stone-100">
          {users.map((user) => (
            <tr key={user.userId} className="hover:bg-stone-50">
              <td className="px-5 py-4 font-semibold text-stone-900">{user.fullName}</td>
              <td className="px-5 py-4 text-stone-600">{user.email}</td>
              <td className="px-5 py-4">
                <Badge className="bg-stone-100 text-stone-700 border-stone-200">{user.role}</Badge>
              </td>
              <td className="px-5 py-4">
                {user.isActive ? (
                  <Badge className="bg-emerald-50 text-emerald-700 border-emerald-200">Activo</Badge>
                ) : (
                  <Badge className="bg-stone-100 text-stone-600 border-stone-200">Inactivo</Badge>
                )}
              </td>
              <td className="px-5 py-4 text-right">
                <Button
                  aria-label={`Acciones para ${user.fullName}`}
                  onClick={() => onEdit(user.userId)}
                  size="sm"
                  variant="ghost"
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
