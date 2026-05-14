namespace SaasCommerce.Contracts.Messaging;

public sealed record TechnicalPing(Guid MessageId, DateTimeOffset OccurredOnUtc);
