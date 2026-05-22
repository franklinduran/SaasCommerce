import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Progress } from '@/components/ui/progress';
import { useSubscriptionUsage } from '../hooks/useSubscriptionUsage';

/**
 * Display subscription resource usage with progress bars
 */
export const SubscriptionUsageCard: React.FC = () => {
  const { usage, isLoading } = useSubscriptionUsage();

  if (isLoading) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Uso de Recursos</CardTitle>
          <CardDescription>Cargando información de uso...</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            {[1, 2, 3, 4].map(i => (
              <div key={i} className="h-12 bg-muted rounded animate-pulse" />
            ))}
          </div>
        </CardContent>
      </Card>
    );
  }

  if (!usage) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Uso de Recursos</CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-muted-foreground">No se pudo cargar la información de uso.</p>
        </CardContent>
      </Card>
    );
  }

  const resources = [
    {
      name: 'Sucursales',
      icon: '🏢',
      current: usage.branches.current,
      maximum: usage.branches.maximum,
      isAtLimit: usage.branches.isAtLimit,
    },
    {
      name: 'Usuarios',
      icon: '👥',
      current: usage.users.current,
      maximum: usage.users.maximum,
      isAtLimit: usage.users.isAtLimit,
    },
    {
      name: 'Productos',
      icon: '📦',
      current: usage.products.current,
      maximum: usage.products.maximum,
      isAtLimit: usage.products.isAtLimit,
    },
    {
      name: 'Ventas/Mes',
      icon: '💰',
      current: usage.sales.current,
      maximum: usage.sales.maximum,
      isAtLimit: usage.sales.isAtLimit,
    },
  ];

  return (
    <Card>
      <CardHeader>
        <CardTitle>Uso de Recursos</CardTitle>
        <CardDescription>
          Monitorea tu uso actual versus los límites de tu plan
        </CardDescription>
      </CardHeader>
      <CardContent>
        <div className="space-y-6">
          {resources.map(resource => {
            const percentage =
              resource.maximum === 0 ? 0 : (resource.current / resource.maximum) * 100;
            const isWarning = percentage >= 80;
            const isError = percentage >= 100;

            return (
              <div key={resource.name} className="space-y-2">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <span className="text-lg">{resource.icon}</span>
                    <span className="font-medium">{resource.name}</span>
                  </div>
                  <div className="text-sm text-muted-foreground">
                    {resource.current} / {resource.maximum}
                  </div>
                </div>
                <Progress
                  value={Math.min(percentage, 100)}
                  className={`h-2 ${
                    isError
                      ? 'bg-red-100'
                      : isWarning
                        ? 'bg-yellow-100'
                        : 'bg-gray-100'
                  }`}
                />
                {isError && (
                  <p className="text-xs text-red-600 font-medium">
                    Has alcanzado el límite de tu plan
                  </p>
                )}
                {isWarning && !isError && (
                  <p className="text-xs text-yellow-600">
                    Estás usando el {Math.round(percentage)}% de tu límite
                  </p>
                )}
              </div>
            );
          })}
        </div>
      </CardContent>
    </Card>
  );
};
