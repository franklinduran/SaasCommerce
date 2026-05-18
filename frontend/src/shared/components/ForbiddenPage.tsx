import { ShieldX } from 'lucide-react'
import { useNavigate } from 'react-router-dom'
import { Button } from '@/shared/components/ui/button'

export function ForbiddenPage() {
  const navigate = useNavigate()

  return (
    <div className="flex h-full flex-col items-center justify-center gap-4 px-6 text-center">
      <ShieldX className="h-16 w-16 text-stone-400" />
      <h2 className="text-2xl font-bold text-stone-900">Acceso denegado</h2>
      <p className="max-w-md text-sm text-stone-600">
        No tienes permisos para acceder a esta seccion. Contacta al administrador si necesitas
        acceso.
      </p>
      <Button onClick={() => navigate('/')} variant="outline">
        Volver al inicio
      </Button>
    </div>
  )
}
