using SaasCommerce.Modules.Billing.Contracts.Responses;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Billing.Application.Invoices;

public interface IGenerateInvoiceUseCase
{
  Task<Result<InvoiceResponse>> Handle(
    GenerateInvoiceCommand command,
    CancellationToken cancellationToken = default);
}
