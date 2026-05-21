import { AlertTriangle, Check, Copy, KeyRound } from 'lucide-react'
import { useState } from 'react'
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/shared/components/ui/alert-dialog'
import { Button } from '@/shared/components/ui/button'

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

  function handleCopy() {
    void navigator.clipboard.writeText(temporaryPassword)
    setCopied(true)
    window.setTimeout(() => setCopied(false), 2000)
  }

  return (
    <AlertDialog onOpenChange={onOpenChange} open={open}>
      <AlertDialogContent className="max-w-md">
        <AlertDialogHeader>
          <div className="flex items-start gap-3">
            <span className="flex h-10 w-10 shrink-0 items-center justify-center rounded-md bg-stone-900 text-white">
              <KeyRound size={18} />
            </span>
            <div>
              <AlertDialogTitle>Contrasena temporal generada</AlertDialogTitle>
              <AlertDialogDescription>
                El usuario debera cambiarla en el proximo inicio de sesion.
              </AlertDialogDescription>
            </div>
          </div>
        </AlertDialogHeader>

        <div className="space-y-3">
          <div className="rounded-md border border-stone-200 bg-stone-50 p-3">
            <p className="text-xs font-semibold uppercase tracking-wide text-stone-500">
              Contrasena temporal
            </p>
            <div className="mt-2 flex items-center gap-2">
              <code className="flex-1 select-all break-all rounded-sm bg-white px-2.5 py-1.5 font-mono text-sm font-bold text-stone-900 ring-1 ring-stone-200">
                {temporaryPassword}
              </code>
              <Button
                aria-label={copied ? 'Copiado' : 'Copiar contrasena'}
                className="shrink-0"
                onClick={handleCopy}
                size="icon"
                type="button"
                variant="secondary"
              >
                {copied ? (
                  <Check className="text-emerald-600" size={16} />
                ) : (
                  <Copy size={16} />
                )}
              </Button>
            </div>
          </div>

          <div className="flex items-start gap-2 rounded-md border border-amber-200 bg-amber-50 p-3">
            <AlertTriangle className="mt-0.5 shrink-0 text-amber-600" size={15} />
            <p className="text-sm font-medium text-amber-900">
              Esta contrasena se muestra solo una vez. Copiala antes de cerrar este dialogo.
            </p>
          </div>
        </div>

        <AlertDialogFooter>
          <AlertDialogCancel>Cancelar</AlertDialogCancel>
          <AlertDialogAction disabled={isLoading} onClick={onConfirm}>
            {isLoading ? 'Guardando...' : 'Listo'}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  )
}
