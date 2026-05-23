import { ArrowLeft, Loader2, Plus, Tag } from 'lucide-react'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useCreateExpenseCategory, useExpenseCategories } from '../hooks/useExpenses'
import { Badge } from '@/shared/components/ui/badge'
import { Button } from '@/shared/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card'
import { Input } from '@/shared/components/ui/input'
import { Label } from '@/shared/components/ui/label'

export function ExpenseCategoriesPage() {
  const navigate = useNavigate()
  const { data: categories, isLoading } = useExpenseCategories()
  const { mutate: createCategory, isPending } = useCreateExpenseCategory()
  const [showForm, setShowForm] = useState(false)
  const [successMsg, setSuccessMsg] = useState<string | null>(null)
  const [errorMsg, setErrorMsg] = useState<string | null>(null)
  const [name, setName] = useState('')
  const [nameError, setNameError] = useState<string | null>(null)

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setNameError(null)
    if (!name || name.trim().length < 2) {
      setNameError('El nombre debe tener al menos 2 caracteres')
      return
    }
    setSuccessMsg(null)
    setErrorMsg(null)
    createCategory(
      { name: name.trim() },
      {
        onSuccess: (cat) => {
          setSuccessMsg(`Categoría "${cat.name}" creada correctamente.`)
          setName('')
          setShowForm(false)
        },
        onError: (err) => {
          setErrorMsg(err instanceof Error ? err.message : 'Error al crear la categoría.')
        },
      },
    )
  }

  return (
    <div className="space-y-6 p-6">
      <div className="flex items-center gap-3">
        <Button variant="ghost" size="icon" onClick={() => navigate('/expenses')}>
          <ArrowLeft size={18} />
        </Button>
        <div className="flex-1">
          <h2 className="text-lg font-semibold text-stone-900">Categorías de gastos</h2>
          <p className="text-sm text-stone-500">Organiza los gastos por categoría.</p>
        </div>
        <Button size="sm" onClick={() => setShowForm((s) => !s)}>
          <Plus size={16} className="mr-1" />
          Nueva categoría
        </Button>
      </div>

      {successMsg && (
        <p className="rounded-md bg-green-50 px-4 py-3 text-sm font-medium text-green-700">
          {successMsg}
        </p>
      )}
      {errorMsg && (
        <p className="rounded-md bg-red-50 px-4 py-3 text-sm font-medium text-red-700">
          {errorMsg}
        </p>
      )}

      {/* New category form */}
      {showForm && (
        <Card className="max-w-md">
          <CardHeader>
            <CardTitle className="text-base">Nueva categoría</CardTitle>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleSubmit} className="space-y-4">
              <div className="space-y-1.5">
                <Label htmlFor="catName">Nombre</Label>
                <Input
                  id="catName"
                  placeholder="Ej. Servicios, Alquiler, Nómina..."
                  value={name}
                  onChange={(e) => {
                    setName(e.target.value)
                    if (nameError) setNameError(null)
                  }}
                />
                {nameError && (
                  <p className="text-xs text-red-600">{nameError}</p>
                )}
              </div>
              <div className="flex gap-2">
                <Button type="submit" size="sm" disabled={isPending}>
                  {isPending ? <Loader2 size={14} className="animate-spin" /> : 'Crear categoría'}
                </Button>
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  onClick={() => {
                    setName('')
                    setNameError(null)
                    setShowForm(false)
                  }}
                >
                  Cancelar
                </Button>
              </div>
            </form>
          </CardContent>
        </Card>
      )}

      {/* Categories list */}
      {isLoading ? (
        <div className="flex h-40 items-center justify-center">
          <Loader2 className="animate-spin text-stone-400" size={24} />
        </div>
      ) : (categories ?? []).length === 0 ? (
        <Card>
          <CardContent className="flex h-40 flex-col items-center justify-center gap-2">
            <Tag className="text-stone-300" size={32} />
            <p className="text-sm text-stone-500">No hay categorías aún.</p>
            <Button size="sm" onClick={() => setShowForm(true)}>
              Crear primera categoría
            </Button>
          </CardContent>
        </Card>
      ) : (
        <Card>
          <CardContent className="p-0">
            <div className="divide-y divide-stone-100">
              {(categories ?? []).map((cat) => (
                <div
                  key={cat.id}
                  className="flex items-center justify-between px-4 py-3"
                >
                  <div className="flex items-center gap-2">
                    <Tag size={16} className="text-stone-400" />
                    <span className="text-sm font-medium text-stone-900">{cat.name}</span>
                  </div>
                  <Badge variant={cat.isActive ? 'default' : 'secondary'}>
                    {cat.isActive ? 'Activa' : 'Inactiva'}
                  </Badge>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  )
}
