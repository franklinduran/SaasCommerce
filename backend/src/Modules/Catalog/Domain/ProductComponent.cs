using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Catalog.Domain;

public sealed class ProductComponent
{
  private ProductComponent()
  {
  }

  public ProductComponent(
    Guid id,
    BusinessId businessId,
    Guid productId,
    Guid componentProductId,
    decimal quantity)
  {
    if (quantity <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(quantity), "Component quantity must be greater than zero.");
    }

    Id = id;
    BusinessId = businessId;
    ProductId = productId;
    ComponentProductId = componentProductId;
    Quantity = quantity;
  }

  public Guid Id { get; private set; }

  public BusinessId BusinessId { get; private set; }

  public Guid ProductId { get; private set; }

  public Guid ComponentProductId { get; private set; }

  public decimal Quantity { get; private set; }
}
