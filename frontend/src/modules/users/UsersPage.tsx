import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Shield, Plus } from 'lucide-react'
import { Button } from '@/shared/components/ui/button'
import { UserTable } from './components/UserTable'
import { usersApi } from './services/usersApi'
import type { UserSummary } from './types'

export default function UsersPage() {
  const navigate = useNavigate()
  const [users, setUsers] = useState<UserSummary[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    loadUsers()
  }, [])

  const loadUsers = async () => {
    setIsLoading(true)
    try {
      const response = await usersApi.getUsers()
      setUsers(response.items)
      setError(null)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error al cargar usuarios')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <div className="bg-stone-100 p-2 rounded-lg">
            <Shield className="h-6 w-6 text-stone-600" />
          </div>
          <div>
            <p className="text-sm text-stone-600">Seguridad</p>
            <h1 className="text-3xl font-bold text-stone-900">Usuarios</h1>
          </div>
        </div>

        <Button onClick={() => navigate('/users/new')} disabled={isLoading}>
          <Plus className="h-4 w-4 mr-2" />
          Crear usuario
        </Button>
      </div>

      {error && (
        <div className="bg-red-50 border border-red-200 rounded-lg p-4">
          <p className="text-sm text-red-700">{error}</p>
        </div>
      )}

      <UserTable
        users={users}
        onEdit={(userId) => navigate(`/users/${userId}`)}
        isLoading={isLoading}
      />
    </div>
  )
}
