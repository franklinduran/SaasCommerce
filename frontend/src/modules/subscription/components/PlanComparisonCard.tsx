import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Check, X } from 'lucide-react';
import { subscriptionApi } from '../services/subscriptionApi';
import { SubscriptionPlanResponse } from '../types';

interface PlanComparisonCardProps {
  currentPlanId?: string;
  onSelectPlan?: (planId: string) => void;
  isLoading?: boolean;
}

/**
 * Display plan comparison in a dialog modal
 */
export const PlanComparisonCard: React.FC<PlanComparisonCardProps> = ({
  currentPlanId,
  onSelectPlan,
  isLoading = false,
}) => {
  const [isOpen, setIsOpen] = useState(false);

  const { data: plans, isLoading: plansLoading } = useQuery({
    queryKey: ['subscription', 'plans'],
    queryFn: subscriptionApi.getPlans,
    staleTime: 1000 * 60 * 30, // 30 minutes
  });

  const handleSelectPlan = (planId: string) => {
    onSelectPlan?.(planId);
    setIsOpen(false);
  };

  const getFeatures = (plans: SubscriptionPlanResponse[]) => {
    const allFeatures = new Set<string>();
    plans.forEach(plan => {
      plan.features.forEach(f => allFeatures.add(f));
    });
    return Array.from(allFeatures).sort();
  };

  const hasFeature = (plan: SubscriptionPlanResponse, featureName: string): boolean => {
    return plan.features.includes(featureName);
  };

  return (
    <Dialog open={isOpen} onOpenChange={setIsOpen}>
      <DialogTrigger asChild>
        <Button variant="outline">Ver Comparativa de Planes</Button>
      </DialogTrigger>
      <DialogContent className="max-w-5xl max-h-[80vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>Comparativa de Planes</DialogTitle>
          <DialogDescription>
            Elige el plan que mejor se adapte a tus necesidades
          </DialogDescription>
        </DialogHeader>

        {plansLoading ? (
          <div className="space-y-4">
            <p className="text-muted-foreground">Cargando planes...</p>
          </div>
        ) : !plans || plans.length === 0 ? (
          <div className="space-y-4">
            <p className="text-muted-foreground">No hay planes disponibles.</p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b">
                  <th className="text-left py-3 px-4 font-semibold">Característica</th>
                  {plans.map(plan => (
                    <th key={plan.id} className="text-center py-3 px-4 font-semibold">
                      <div className="font-bold text-base">{plan.name}</div>
                      <div className="text-muted-foreground">
                        ${plan.monthlyPrice}/mes
                      </div>
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {/* Pricing Row */}
                <tr className="border-b hover:bg-muted/50">
                  <td className="py-3 px-4">Precio mensual</td>
                  {plans.map(plan => (
                    <td key={plan.id} className="text-center py-3 px-4">
                      <div className="font-bold text-lg">${plan.monthlyPrice}</div>
                    </td>
                  ))}
                </tr>

                {/* Limits */}
                <tr className="border-b hover:bg-muted/50">
                  <td className="py-3 px-4">Sucursales</td>
                  {plans.map(plan => (
                    <td key={plan.id} className="text-center py-3 px-4">
                      {plan.maxBranches === 999 ? '∞' : plan.maxBranches}
                    </td>
                  ))}
                </tr>

                <tr className="border-b hover:bg-muted/50">
                  <td className="py-3 px-4">Usuarios</td>
                  {plans.map(plan => (
                    <td key={plan.id} className="text-center py-3 px-4">
                      {plan.maxUsers === 999 ? '∞' : plan.maxUsers}
                    </td>
                  ))}
                </tr>

                <tr className="border-b hover:bg-muted/50">
                  <td className="py-3 px-4">Productos</td>
                  {plans.map(plan => (
                    <td key={plan.id} className="text-center py-3 px-4">
                      {plan.maxProducts === 999999 ? '∞' : plan.maxProducts.toLocaleString()}
                    </td>
                  ))}
                </tr>

                <tr className="border-b hover:bg-muted/50">
                  <td className="py-3 px-4">Ventas/mes</td>
                  {plans.map(plan => (
                    <td key={plan.id} className="text-center py-3 px-4">
                      {plan.maxSalesPerMonth === 999999
                        ? '∞'
                        : plan.maxSalesPerMonth.toLocaleString()}
                    </td>
                  ))}
                </tr>

                {/* Features */}
                {getFeatures(plans).map(featureName => (
                  <tr key={featureName} className="border-b hover:bg-muted/50">
                    <td className="py-3 px-4">{featureName}</td>
                    {plans.map(plan => (
                      <td key={plan.id} className="text-center py-3 px-4">
                        {hasFeature(plan, featureName) ? (
                          <Check className="w-5 h-5 text-green-600 mx-auto" />
                        ) : (
                          <X className="w-5 h-5 text-gray-300 mx-auto" />
                        )}
                      </td>
                    ))}
                  </tr>
                ))}

                {/* Action Row */}
                <tr>
                  <td className="py-4 px-4"></td>
                  {plans.map(plan => (
                    <td key={plan.id} className="text-center py-4 px-4">
                      <Button
                        onClick={() => handleSelectPlan(plan.id)}
                        disabled={
                          isLoading ||
                          currentPlanId === plan.id
                        }
                        variant={currentPlanId === plan.id ? 'outline' : 'default'}
                      >
                        {currentPlanId === plan.id
                          ? 'Plan Actual'
                          : 'Seleccionar'}
                      </Button>
                    </td>
                  ))}
                </tr>
              </tbody>
            </table>
          </div>
        )}
      </DialogContent>
    </Dialog>
  );
};
