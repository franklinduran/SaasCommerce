namespace SaasCommerce.Modules.Sales.Contracts.Requests;

public sealed record OpenCashRegisterRequest(Guid BranchId, decimal OpeningAmount, string? Notes);

public sealed record RegisterCashRegisterMovementRequest(string Type, decimal Amount, string Reason);

public sealed record CloseCashRegisterRequest(decimal CountedAmount, string? CloseNotes);
