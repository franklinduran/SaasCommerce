import { Badge } from '@/components/ui/badge';
import { SubscriptionStatus } from '../types';

interface SubscriptionStatusBadgeProps {
  status: SubscriptionStatus;
  className?: string;
}

/**
 * Display subscription status with appropriate color and styling
 */
export const SubscriptionStatusBadge: React.FC<SubscriptionStatusBadgeProps> = ({
  status,
  className = '',
}) => {
  const getStatusConfig = (status: SubscriptionStatus) => {
    switch (status) {
      case SubscriptionStatus.Trial:
        return {
          variant: 'secondary' as const,
          label: 'Período de Prueba',
          description: 'Acceso completo durante 14 días',
        };
      case SubscriptionStatus.Active:
        return {
          variant: 'default' as const,
          label: 'Activo',
          description: 'Tu suscripción está activa',
        };
      case SubscriptionStatus.PastDue:
        return {
          variant: 'destructive' as const,
          label: 'Vencimiento Próximo',
          description: 'Por favor, renueva tu suscripción',
        };
      case SubscriptionStatus.Suspended:
        return {
          variant: 'destructive' as const,
          label: 'Suspendido',
          description: 'Tu cuenta ha sido suspendida',
        };
      case SubscriptionStatus.Expired:
        return {
          variant: 'outline' as const,
          label: 'Expirado',
          description: 'Tu suscripción ha expirado',
        };
      case SubscriptionStatus.Cancelled:
        return {
          variant: 'outline' as const,
          label: 'Cancelado',
          description: 'Tu suscripción fue cancelada',
        };
      default:
        return {
          variant: 'outline' as const,
          label: 'Desconocido',
          description: 'Estado desconocido',
        };
    }
  };

  const config = getStatusConfig(status);

  return (
    <div className={className}>
      <Badge variant={config.variant}>{config.label}</Badge>
      <p className="text-sm text-muted-foreground mt-1">{config.description}</p>
    </div>
  );
};
