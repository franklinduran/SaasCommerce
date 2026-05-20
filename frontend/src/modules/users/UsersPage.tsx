import { useCallback, useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Plus, Users } from 'lucide-react'
import { Button } from '@/shared/components/ui/button'
import { Card } from '@/shared/components/ui/card'
import { UserTable } from './components/UserTable'
import { usersApi } from './services/usersApi'
import type { UserSummary } from './types'

export default function UsersPage() {
  const navigate = useNavigate()
  const [users, setUsers] = useState<UserSummary[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const loadUsers = useCallback(async () => {
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
  }, [])

  useEffect(() => {
    loadUsers()
  }, [loadUsers])

  return (
    <section className="space-y-5 p-6 lg:p-8">
      <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
        <div>
          <p className="flex items-center gap-2 text-sm font-semibold uppercase text-stone-500">
            <Users size={16} />
            Seguridad
          </p>
          <h2 className="mt-1 text-2xl font-semibold text-stone-950">Usuarios</h2>
          <p className="mt-2 max-w-2xl text-sm font-medium text-stone-600">
            Administra cuentas, roles y accesos del personal del negocio.
          </p>
        </div>
        <Button disabled={isLoading} onClick={() => navigate('/users/new')} type="button">
          <Plus size={16} />
          Crear usuario
        </Button>
      </div>

      {error && (
        <Card className="border-red-200 bg-red-50 ring-red-200">
          <p className="p-5 text-sm font-semibold text-red-700">{error}</p>
        </Card>
      )}

      <Card>
        <UserTable
          users={users}
          onEdit={(userId) => navigate(`/users/${userId}`)}
          isLoading={isLoading}
        />
      </Card>
    </section>
  )
}
