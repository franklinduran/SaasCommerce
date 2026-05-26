using System.Globalization;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Customers.Application.Abstractions;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Customers.Application.Credits;

public sealed record ExportCustomerCreditsCsvQuery;

public sealed class ExportCustomerCreditsCsvHandler(
  ICustomerCreditRepository credits,
  ICustomerRepository customers,
  ICurrentUserService currentUser)
{
  private static readonly IReadOnlyList<string> Headers =
  [
    "Cliente", "Límite Crédito", "Balance Actual", "Estado", "Creado", "Actualizado"
  ];

  public async Task<Result<byte[]>> Handle(
    ExportCustomerCreditsCsvQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<byte[]>(new DomainError("EXPORT_USER_CONTEXT_REQUIRED",
        "Se requiere contexto de usuario autenticado para exportar."));
    }

    var bId = new BusinessId(businessId);
    var accounts = await credits.ExportAllAccountsAsync(bId, cancellationToken);

    if (accounts.Count == 0)
    {
      return Result.Success(CsvBuilder.Build(Headers, []));
    }

    var allCustomers = await customers.ExportAllAsync(bId, cancellationToken);
    var customerDict = allCustomers.ToDictionary(c => c.Id, c => c.FullName);

    var rows = accounts.Select(a =>
    {
      var customerName = customerDict.TryGetValue(a.CustomerId, out var name)
        ? name
        : a.CustomerId.ToString();

      return (IReadOnlyCollection<string?>)
      [
        customerName!,
        a.HasUnlimitedCredit ? "Ilimitado" : a.CreditLimit.ToString("F2", CultureInfo.InvariantCulture),
        a.CurrentBalance.ToString("F2", CultureInfo.InvariantCulture),
        a.Status.ToString(),
        a.CreatedAt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        a.UpdatedAt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
      ];
    });

    var csv = CsvBuilder.Build(Headers, rows);
    return Result.Success(csv);
  }
}
