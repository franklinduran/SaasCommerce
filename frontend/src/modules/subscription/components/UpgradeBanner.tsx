import { TrendingUp } from 'lucide-react';
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert';
import { Button } from '@/components/ui/button';
import { useSubscriptionUsage } from '../hooks/useSubscriptionUsage';

interface UpgradeBannerProps {
  onUpgradeClick?: () => void;
}

/**
 * Display upgrade recommendation banner when resources are near limits
 */
export const UpgradeBanner: React.FC<UpgradeBannerProps> = ({ onUpgradeClick }) => {
  const { usage, isNearLimit } = useSubscriptionUsage();

  if (!usage) {
    return null;
  }

  const nearLimitResources = ['branches', 'users', 'products', 'sales'].filter(
    resource => isNearLimit(resource as 'branches' | 'users' | 'products' | 'sales')
  );

  if (nearLimitResources.length === 0) {
    return null;
  }

  const resourceLabels: Record<string, string> = {
    branches: 'Sucursales',
    users: 'Usuarios',
    products: 'Productos',
    sales: 'Ventas',
  };

  return (
    <Alert className="border-yellow-200 bg-yellow-50">
      <TrendingUp className="h-4 w-4 text-yellow-600" />
      <AlertTitle className="text-yellow-900">Estás usando muchos recursos</AlertTitle>
      <AlertDescription className="text-yellow-800">
        <p className="mb-3">
          Tu uso de{' '}
          {nearLimitResources.map((r, i) => (
            <span key={r}>
              {i > 0 && i === nearLimitResources.length - 1 ? ' y ' : i > 0 ? ', ' : ''}
              <strong>{resourceLabels[r]}</strong>
            </span>
          ))}{' '}
          está cerca del límite de tu plan actual. Considera cambiar a un plan superior para
          evitar interrupciones.
        </p>
        <Button
          onClick={onUpgradeClick}
          size="sm"
          variant="outline"
          className="bg-yellow-100 border-yellow-300 hover:bg-yellow-200"
        >
          Ver planes disponibles
        </Button>
      </AlertDescription>
    </Alert>
  );
};
