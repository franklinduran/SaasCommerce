using System.Globalization;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Cash;

public sealed record ExportCashSessionsCsvQuery(
  DateTimeOffset? DateFrom,
  DateTimeOffset? DateTo);

public sealed class ExportCashSessionsCsvHandler(
  ICashSessionRepository cashSessions,
  ICurrentUserService currentUser)
{
  private static readonly IReadOnlyList<string> Headers =
  [
    "Fecha Apertura", "Fecha Cierre", "Estado", "Balance Apertura",
    "Balance Sistema", "Balance Cierre", "Diferencia", "Notas"
  ];

  public async Task<Result<byte[]>> Handle(
    ExportCashSessionsCsvQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<byte[]>(new DomainError("EXPORT_USER_CONTEXT_REQUIRED",
        "Se requiere contexto de usuario autenticado para exportar."));
    }

    var items = await cashSessions.ExportAllAsync(
      new BusinessId(businessId),
      query.DateFrom,
      query.DateTo,
      cancellationToken);

    var rows = items.Select(s =>
    {
      var difference = s.ClosingBalance.HasValue
        ? s.ClosingBalance.Value - s.SystemBalance
        : (decimal?)null;

      return (IReadOnlyCollection<string?>)
      [
        s.OpenedAt.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
        s.ClosedAt?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
        s.Status.ToString(),
        s.OpeningBalance.ToString("F2", CultureInfo.InvariantCulture),
        s.SystemBalance.ToString("F2", CultureInfo.InvariantCulture),
        s.ClosingBalance?.ToString("F2", CultureInfo.InvariantCulture),
        difference?.ToString("F2", CultureInfo.InvariantCulture),
        s.Notes
      ];
    });

    var csv = CsvBuilder.Build(Headers, rows);
    return Result.Success(csv);
  }
}
