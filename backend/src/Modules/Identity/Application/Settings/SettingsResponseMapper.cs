using SaasCommerce.Modules.Identity.Contracts.Responses;
using SaasCommerce.Modules.Identity.Domain;
using SaasCommerce.Modules.Tenancy.Domain;

namespace SaasCommerce.Modules.Identity.Application.Settings;

internal static class SettingsResponseMapper
{
  public static CurrentUserResponse ToCurrentUserResponse(
    User user,
    Business business,
    Branch? branch)
  {
    ArgumentNullException.ThrowIfNull(user);
    ArgumentNullException.ThrowIfNull(business);

    return new CurrentUserResponse(
      user.Id,
      user.FullName,
      user.Email,
      user.Phone,
      user.Roles.Select(role => role.Name).Order(StringComparer.Ordinal).ToArray(),
      new CurrentUserBusinessResponse(
        business.Id.Value,
        business.Name,
        business.IdentificationType?.ToString(),
        business.IdentificationNumber),
      branch is null ? null : new CurrentUserBranchResponse(branch.Id.Value, branch.Name));
  }

  public static CurrentBusinessResponse ToCurrentBusinessResponse(Business business)
  {
    ArgumentNullException.ThrowIfNull(business);

    return new CurrentBusinessResponse(
      business.Id.Value,
      business.Name,
      business.IdentificationType?.ToString(),
      business.IdentificationNumber,
      business.Phones
        .OrderByDescending(phone => phone.IsPrimary)
        .ThenBy(phone => phone.Label, StringComparer.Ordinal)
        .Select(phone => new BusinessPhoneResponse(phone.Number, phone.Label, phone.IsPrimary))
        .ToArray());
  }

  public static CurrentBranchResponse ToCurrentBranchResponse(Branch branch)
  {
    ArgumentNullException.ThrowIfNull(branch);

    return new CurrentBranchResponse(
      branch.Id.Value,
      branch.BusinessId.Value,
      branch.Name,
      branch.Address,
      branch.Phone);
  }
}
