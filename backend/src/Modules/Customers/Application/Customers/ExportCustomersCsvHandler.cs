using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Customers.Application.Abstractions;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Customers.Application.Customers;

public sealed record ExportCustomersCsvQuery;

public sealed class ExportCustomersCsvHandler(
  ICustomerRepository customers,
  ICurrentUserService currentUser)
{
  private static readonly IReadOnlyList<string> Headers =
  [
    "Nombre", "Apellido", "Nombre Completo", "Teléfono", "Email", "Activo", "Creado"
  ];

  public async Task<Result<byte[]>> Handle(
    ExportCustomersCsvQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<byte[]>(new DomainError("EXPORT_USER_CONTEXT_REQUIRED",
        "Se requiere contexto de usuario autenticado para exportar."));
    }

    var items = await customers.ExportAllAsync(new BusinessId(businessId), cancellationToken);

    var rows = items.Select(c => (IReadOnlyCollection<string?>)
    [
      c.FirstName,
      c.LastName,
      c.FullName,
      c.Phone,
      c.Email,
      c.IsActive ? "Sí" : "No",
      c.CreatedAt.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)
    ]);

    var csv = CsvBuilder.Build(Headers, rows);
    return Result.Success(csv);
  }
}
