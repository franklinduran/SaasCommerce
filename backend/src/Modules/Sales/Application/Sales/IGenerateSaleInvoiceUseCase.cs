using SaasCommerce.Modules.Billing.Contracts.Events.V1;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Sales.Application.Sales;

public interface IGenerateSaleInvoiceUseCase
{
  Task<Result> ExecuteAsync(
    InvoiceGenerationRequestedEventV1 invoiceGenerationRequested,
    CancellationToken cancellationToken = default);
}
