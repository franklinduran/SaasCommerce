import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { ArrowRight, Phone } from 'lucide-react';
import { useSubscription } from '../hooks/useSubscription';
import { useSubscriptionUsage } from '../hooks/useSubscriptionUsage';
import { SubscriptionStatusBadge } from '../components/SubscriptionStatusBadge';
import { SubscriptionUsageCard } from '../components/SubscriptionUsageCard';
import { SubscriptionAlertBanner } from '../components/SubscriptionAlertBanner';
import { UpgradeBanner } from '../components/UpgradeBanner';
import { PlanComparisonCard } from '../components/PlanComparisonCard';
import { formatDate, formatDistanceToNow } from 'date-fns';
import { es } from 'date-fns/locale';

/**
 * Main subscription management page
 */
export const SubscriptionPage: React.FC = () => {
  const { subscription, changePlan, isChangingPlan, cancelSubscription, isCancelling, reactivateSubscription, isReactivating } = useSubscription();
  useSubscriptionUsage();

  if (!subscription) {
    return (
      <div className="space-y-4">
        <div className="h-12 bg-muted rounded animate-pulse" />
        <div className="h-64 bg-muted rounded animate-pulse" />
      </div>
    );
  }

  const handlePlanChange = async (newPlanId: string) => {
    try {
      await changePlan(newPlanId);
      // Show success toast
    } catch (error) {
      console.error('Error changing plan:', error);
      // Show error toast
    }
  };

  const handleCancel = async () => {
    if (confirm('¿Estás seguro de que quieres cancelar tu suscripción?')) {
      try {
        await cancelSubscription();
        // Show success toast
      } catch (error) {
        console.error('Error cancelling subscription:', error);
        // Show error toast
      }
    }
  };

  const handleReactivate = async () => {
    try {
      await reactivateSubscription();
      // Show success toast
    } catch (error) {
      console.error('Error reactivating subscription:', error);
      // Show error toast
    }
  };

  const trialEndsAt = subscription.trialEndsAt
    ? new Date(subscription.trialEndsAt)
    : null;
  const periodEndsAt = subscription.currentPeriodEnd
    ? new Date(subscription.currentPeriodEnd)
    : null;

  return (
    <div className="space-y-6">
      {/* Alerts */}
      <SubscriptionAlertBanner
        onReactivateClick={handleReactivate}
        onContactSupport={() => {
          // Open support contact
        }}
      />

      <UpgradeBanner onUpgradeClick={() => {
        // Scroll to comparison
      }} />

      {/* Main Subscription Info */}
      <Card>
        <CardHeader>
          <div className="flex items-start justify-between">
            <div>
              <CardTitle className="text-3xl">{subscription.plan.name}</CardTitle>
              <CardDescription className="mt-2">
                ${subscription.plan.monthlyPrice.toFixed(2)}/mes
              </CardDescription>
            </div>
            <SubscriptionStatusBadge status={subscription.status} />
          </div>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            {/* Plan Details */}
            <div className="space-y-4">
              <h3 className="font-semibold">Límites de tu plan</h3>
              <ul className="space-y-2 text-sm">
                <li className="flex justify-between">
                  <span className="text-muted-foreground">Sucursales:</span>
                  <strong>
                    {subscription.plan.maxBranches === 999 ? 'Ilimitadas' : subscription.plan.maxBranches}
                  </strong>
                </li>
                <li className="flex justify-between">
                  <span className="text-muted-foreground">Usuarios:</span>
                  <strong>
                    {subscription.plan.maxUsers === 999 ? 'Ilimitados' : subscription.plan.maxUsers}
                  </strong>
                </li>
                <li className="flex justify-between">
                  <span className="text-muted-foreground">Productos:</span>
                  <strong>
                    {subscription.plan.maxProducts === 999999
                      ? 'Ilimitados'
                      : subscription.plan.maxProducts.toLocaleString()}
                  </strong>
                </li>
                <li className="flex justify-between">
                  <span className="text-muted-foreground">Ventas/mes:</span>
                  <strong>
                    {subscription.plan.maxSalesPerMonth === 999999
                      ? 'Ilimitadas'
                      : subscription.plan.maxSalesPerMonth.toLocaleString()}
                  </strong>
                </li>
              </ul>
            </div>

            {/* Dates */}
            <div className="space-y-4">
              <h3 className="font-semibold">Detalles de suscripción</h3>
              <ul className="space-y-2 text-sm">
                {subscription.status === 'Trial' && trialEndsAt && (
                  <>
                    <li className="flex justify-between">
                      <span className="text-muted-foreground">Período de prueba:</span>
                      <strong>
                        {formatDistanceToNow(trialEndsAt, { locale: es })}
                      </strong>
                    </li>
                    <li className="flex justify-between">
                      <span className="text-muted-foreground">Vence el:</span>
                      <strong>{formatDate(trialEndsAt, 'PPP', { locale: es })}</strong>
                    </li>
                  </>
                )}
                {(subscription.status === 'Active' || subscription.status === 'PastDue') &&
                  periodEndsAt && (
                    <>
                      <li className="flex justify-between">
                        <span className="text-muted-foreground">Período actual:</span>
                        <strong>
                          {formatDistanceToNow(periodEndsAt, { locale: es })}
                        </strong>
                      </li>
                      <li className="flex justify-between">
                        <span className="text-muted-foreground">Vence el:</span>
                        <strong>{formatDate(periodEndsAt, 'PPP', { locale: es })}</strong>
                      </li>
                    </>
                  )}
                <li className="flex justify-between">
                  <span className="text-muted-foreground">Iniciada:</span>
                  <strong>{formatDate(new Date(subscription.startedAt), 'PPP', { locale: es })}</strong>
                </li>
              </ul>
            </div>
          </div>

          {/* Features */}
          {subscription.plan.features.length > 0 && (
            <div className="mt-6 pt-6 border-t">
              <h3 className="font-semibold mb-3">Características incluidas</h3>
              <div className="grid grid-cols-2 md:grid-cols-3 gap-2">
                {subscription.plan.features.map(feature => (
                  <div key={feature} className="flex items-center gap-2">
                    <div className="w-2 h-2 bg-green-600 rounded-full" />
                    <span className="text-sm">{feature}</span>
                  </div>
                ))}
              </div>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Usage */}
      <SubscriptionUsageCard />

      {/* Plans Comparison */}
      <Card>
        <CardHeader>
          <CardTitle>Cambiar de plan</CardTitle>
          <CardDescription>
            Mejora a un plan superior o reduce tu plan según tus necesidades
          </CardDescription>
        </CardHeader>
        <CardContent>
          <PlanComparisonCard
            currentPlanId={subscription.plan.id}
            onSelectPlan={handlePlanChange}
            isLoading={isChangingPlan}
          />
        </CardContent>
      </Card>

      {/* Actions */}
      <Tabs defaultValue="general" className="w-full">
        <TabsList>
          <TabsTrigger value="general">Configuración</TabsTrigger>
          <TabsTrigger value="support">Soporte</TabsTrigger>
        </TabsList>

        <TabsContent value="general" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle className="text-base">Acciones de suscripción</CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
              {subscription.status !== 'Cancelled' && (
                <Button
                  variant="destructive"
                  onClick={handleCancel}
                  disabled={isCancelling}
                  className="w-full"
                >
                  {isCancelling ? 'Cancelando...' : 'Cancelar suscripción'}
                </Button>
              )}
              {subscription.status === 'Cancelled' && (
                <Button
                  onClick={handleReactivate}
                  disabled={isReactivating}
                  className="w-full"
                >
                  {isReactivating ? 'Reactivando...' : 'Reactivar suscripción'}
                </Button>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="support" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle className="text-base">¿Necesitas ayuda?</CardTitle>
              <CardDescription>
                Nuestro equipo está disponible para responder tus preguntas
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-3">
              <Button variant="outline" className="w-full" onClick={() => {
                // Open live chat or contact form
              }}>
                <Phone className="w-4 h-4 mr-2" />
                Contactar soporte
              </Button>
              <Button variant="outline" className="w-full" onClick={() => {
                // Navigate to help docs
              }}>
                <ArrowRight className="w-4 h-4 mr-2" />
                Ver documentación
              </Button>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
};
