import { useState } from 'react';
import { AlertCircle, Clock, AlertTriangle } from 'lucide-react';
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert';
import { Button } from '@/components/ui/button';
import { useSubscription } from '../hooks/useSubscription';
import { SubscriptionStatus } from '../types';
import { formatDistanceToNow, parseISO } from 'date-fns';
import { es } from 'date-fns/locale';

interface SubscriptionAlertBannerProps {
  onReactivateClick?: () => void;
  onContactSupport?: () => void;
}

/**
 * Display alerts for subscription status issues
 */
export const SubscriptionAlertBanner: React.FC<SubscriptionAlertBannerProps> = ({
  onReactivateClick,
  onContactSupport,
}) => {
  const { subscription } = useSubscription();
  const [dismissed, setDismissed] = useState(false);

  if (!subscription || dismissed) {
    return null;
  }

  const now = new Date();
  const trialEndsAt = subscription.trialEndsAt ? parseISO(subscription.trialEndsAt) : null;
  const periodEndsAt = subscription.currentPeriodEnd ? parseISO(subscription.currentPeriodEnd) : null;

  const isTrialExpiringSoon =
    subscription.status === SubscriptionStatus.Trial &&
    trialEndsAt &&
    trialEndsAt.getTime() - now.getTime() < 7 * 24 * 60 * 60 * 1000; // 7 days

  const isPeriodExpiringSoon =
    (subscription.status === SubscriptionStatus.Active ||
      subscription.status === SubscriptionStatus.PastDue) &&
    periodEndsAt &&
    periodEndsAt.getTime() - now.getTime() < 7 * 24 * 60 * 60 * 1000; // 7 days

  const isExpired =
    subscription.status === SubscriptionStatus.Expired ||
    (periodEndsAt && periodEndsAt < now) ||
    (trialEndsAt && trialEndsAt < now);

  const isSuspended = subscription.status === SubscriptionStatus.Suspended;
  const isCancelled = subscription.status === SubscriptionStatus.Cancelled;
  const isPastDue = subscription.status === SubscriptionStatus.PastDue;

  // Trial expiring soon
  if (isTrialExpiringSoon && trialEndsAt) {
    return (
      <Alert className="border-blue-200 bg-blue-50">
        <Clock className="h-4 w-4 text-blue-600" />
        <AlertTitle className="text-blue-900">Tu período de prueba vence pronto</AlertTitle>
        <AlertDescription className="text-blue-800">
          <p className="mb-3">
            Te quedan{' '}
            <strong>
              {formatDistanceToNow(trialEndsAt, { locale: es, addSuffix: false })}
            </strong>{' '}
            en tu período de prueba. Después deberás elegir un plan para continuar usando ComercioFlow.
          </p>
          <div className="space-x-2">
            <Button size="sm" variant="outline" className="bg-blue-100 border-blue-300 hover:bg-blue-200">
              Elegir plan
            </Button>
            <Button
              size="sm"
              variant="ghost"
              onClick={() => setDismissed(true)}
            >
              Descartar
            </Button>
          </div>
        </AlertDescription>
      </Alert>
    );
  }

  // Period expiring soon
  if (isPeriodExpiringSoon && periodEndsAt) {
    return (
      <Alert className="border-blue-200 bg-blue-50">
        <Clock className="h-4 w-4 text-blue-600" />
        <AlertTitle className="text-blue-900">Tu suscripción vence pronto</AlertTitle>
        <AlertDescription className="text-blue-800">
          <p className="mb-3">
            Tu período de facturación vence en{' '}
            <strong>
              {formatDistanceToNow(periodEndsAt, { locale: es, addSuffix: false })}
            </strong>
            . Por favor, renueva tu suscripción a tiempo.
          </p>
          <Button size="sm" variant="outline" className="bg-blue-100 border-blue-300 hover:bg-blue-200">
            Renovar suscripción
          </Button>
        </AlertDescription>
      </Alert>
    );
  }

  // Expired
  if (isExpired) {
    return (
      <Alert className="border-red-200 bg-red-50">
        <AlertTriangle className="h-4 w-4 text-red-600" />
        <AlertTitle className="text-red-900">Tu suscripción ha expirado</AlertTitle>
        <AlertDescription className="text-red-800">
          <p className="mb-3">
            Tu suscripción expiró. Por favor, renuévala para continuar usando todas las funciones de
            ComercioFlow.
          </p>
          <div className="space-x-2">
            <Button size="sm" className="bg-red-600 hover:bg-red-700">
              Renovar ahora
            </Button>
            <Button size="sm" variant="ghost" onClick={onContactSupport}>
              Contactar soporte
            </Button>
          </div>
        </AlertDescription>
      </Alert>
    );
  }

  // Suspended
  if (isSuspended) {
    return (
      <Alert className="border-red-200 bg-red-50">
        <AlertCircle className="h-4 w-4 text-red-600" />
        <AlertTitle className="text-red-900">Tu cuenta está suspendida</AlertTitle>
        <AlertDescription className="text-red-800">
          <p className="mb-3">
            Tu cuenta ha sido suspendida. Todas las operaciones están bloqueadas. Por favor,
            contacta a nuestro equipo de soporte para resolver esto.
          </p>
          <Button size="sm" variant="outline" className="bg-red-100 border-red-300 hover:bg-red-200" onClick={onContactSupport}>
            Contactar soporte
          </Button>
        </AlertDescription>
      </Alert>
    );
  }

  // Cancelled
  if (isCancelled) {
    return (
      <Alert className="border-orange-200 bg-orange-50">
        <AlertTriangle className="h-4 w-4 text-orange-600" />
        <AlertTitle className="text-orange-900">Tu suscripción fue cancelada</AlertTitle>
        <AlertDescription className="text-orange-800">
          <p className="mb-3">
            Tu suscripción fue cancelada. Puedes reactivarla en cualquier momento para volver a usar
            ComercioFlow.
          </p>
          <Button size="sm" onClick={onReactivateClick} className="bg-orange-600 hover:bg-orange-700">
            Reactivar suscripción
          </Button>
        </AlertDescription>
      </Alert>
    );
  }

  // Past Due
  if (isPastDue) {
    return (
      <Alert className="border-yellow-200 bg-yellow-50">
        <Clock className="h-4 w-4 text-yellow-600" />
        <AlertTitle className="text-yellow-900">Pago vencido</AlertTitle>
        <AlertDescription className="text-yellow-800">
          <p className="mb-3">
            Tu pago está vencido. Por favor, actualiza tu información de pago para evitar la
            suspensión de tu cuenta.
          </p>
          <Button size="sm" variant="outline" className="bg-yellow-100 border-yellow-300 hover:bg-yellow-200">
            Actualizar pago
          </Button>
        </AlertDescription>
      </Alert>
    );
  }

  return null;
};
