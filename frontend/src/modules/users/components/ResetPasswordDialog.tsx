import { useState } from 'react'
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogHeader, AlertDialogTitle } from '@/shared/components/ui/alert-dialog'
import { Button } from '@/shared/components/ui/button'
import { Copy, Check } from 'lucide-react'

interface ResetPasswordDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  temporaryPassword: string
  isLoading?: boolean
  onConfirm: () => void
}

export function ResetPasswordDialog({
  open,
  onOpenChange,
  temporaryPassword,
  isLoading,
  onConfirm,
}: Readonly<ResetPasswordDialogProps>) {
  const [copied, setCopied] = useState(false)

  const handleCopy = () => {
    navigator.clipboard.writeText(temporaryPassword)
    setCopied(true)
    setTimeout(() => setCopied(false), 2000)
  }

  return (
    <AlertDialog open={open} onOpenChange={onOpenChange}>
      <AlertDialogContent className="max-w-md">
        <AlertDialogHeader>
          <AlertDialogTitle>Contraseña temporal generada</AlertDialogTitle>
          <AlertDialogDescription>
            Se ha generado una contraseña temporal. El usuario deberá cambiarla en el próximo login.
          </AlertDialogDescription>
        </AlertDialogHeader>

        <div className="space-y-4">
          <div className="bg-stone-50 border border-stone-200 rounded-lg p-4">
            <p className="text-sm text-stone-600 mb-2">Contraseña temporal:</p>
            <div className="flex items-center gap-2">
              <code className="flex-1 font-mono font-bold text-stone-900 break-all">{temporaryPassword}</code>
              <Button
                variant="ghost"
                size="sm"
                onClick={handleCopy}
                className="flex-shrink-0"
              >
                {copied ? (
                  <Check className="h-4 w-4 text-emerald-600" />
                ) : (
                  <Copy className="h-4 w-4" />
                )}
              </Button>
            </div>
          </div>

          <div className="bg-amber-50 border border-amber-200 rounded-lg p-3">
            <p className="text-sm text-amber-800">
              ⚠️ Esta contraseña se muestra solo una vez. Cópiala antes de cerrar este diálogo.
            </p>
          </div>
        </div>

        <div className="flex justify-end gap-2">
          <AlertDialogCancel>Cancelar</AlertDialogCancel>
          <AlertDialogAction onClick={onConfirm} disabled={isLoading}>
            {isLoading ? 'Guardando...' : 'Confirmar'}
          </AlertDialogAction>
        </div>
      </AlertDialogContent>
    </AlertDialog>
  )
}
