using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using SaasCommerce.BuildingBlocks.Contracts.Common;
using SaasCommerce.Modules.Identity.Contracts.Requests;
using SaasCommerce.Modules.Identity.Contracts.Responses;

namespace SaasCommerce.Api.Tests;

public sealed class AuthEndpointTests
{
  [Fact]
  public async Task MeShouldReturnUnauthorizedWithoutToken()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();

    var response = await client.GetAsync("/api/me");
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeFalse();
    payload.Data.Should().BeNull();
    payload.Error.Should().NotBeNull();
    payload.Error!.Code.Should().Be("unauthorized");
  }

  [Fact]
  public async Task MeShouldReturnCurrentUserWithValidToken()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    var loginResponse = await client.PostAsJsonAsync(
      "/api/auth/login",
      new LoginRequest("admin@test.com", "Admin123!"));
    var login = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();

    loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    login.Should().NotBeNull();
    login!.IsSuccess.Should().BeTrue();
    login.Data.Should().NotBeNull();

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
      "Bearer",
      login.Data!.AccessToken);

    var response = await client.GetAsync("/api/me");

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var me = await response.Content.ReadFromJsonAsync<ApiResponse<CurrentUserResponse>>();
    me.Should().NotBeNull();
    me!.IsSuccess.Should().BeTrue();
    me.Data.Should().NotBeNull();
    me.Data!.Business.BusinessId.Should().Be(Guid.Parse("11111111-1111-1111-1111-111111111111"));
    me.Data.Branch.Should().NotBeNull();
    me.Data.Branch!.BranchId.Should().Be(Guid.Parse("22222222-2222-2222-2222-222222222222"));
    me.Data.Roles.Should().ContainSingle().Which.Should().Be("Admin");
  }

  [Fact]
  public async Task SettingsEndpointsShouldReturnCurrentTenantDataWithValidToken()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);

    var businessResponse = await client.GetAsync("/api/business/current");
    var branchResponse = await client.GetAsync("/api/branches/current");

    businessResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    branchResponse.StatusCode.Should().Be(HttpStatusCode.OK);

    var business = await businessResponse.Content.ReadFromJsonAsync<ApiResponse<CurrentBusinessResponse>>();
    var branch = await branchResponse.Content.ReadFromJsonAsync<ApiResponse<CurrentBranchResponse>>();

    business.Should().NotBeNull();
    business!.IsSuccess.Should().BeTrue();
    business.Data!.BusinessId.Should().Be(Guid.Parse("11111111-1111-1111-1111-111111111111"));
    business.Data.Phones.Should().ContainSingle(phone => phone.IsPrimary);

    branch.Should().NotBeNull();
    branch!.IsSuccess.Should().BeTrue();
    branch.Data!.BranchId.Should().Be(Guid.Parse("22222222-2222-2222-2222-222222222222"));
  }

  [Fact]
  public async Task ProfileAndBranchShouldUpdateOnlyAuthenticatedContext()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);

    var profileResponse = await client.PutAsJsonAsync(
      "/api/me/profile",
      new UpdateMyProfileRequest("Admin Actualizado", "8091112222"));
    var branchResponse = await client.PutAsJsonAsync(
      "/api/branches/current",
      new UpdateCurrentBranchRequest("Sucursal Actualizada", "La Vega", "8093334444"));

    profileResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    branchResponse.StatusCode.Should().Be(HttpStatusCode.OK);

    var profile = await profileResponse.Content.ReadFromJsonAsync<ApiResponse<CurrentUserResponse>>();
    var branch = await branchResponse.Content.ReadFromJsonAsync<ApiResponse<CurrentBranchResponse>>();

    profile!.Data!.FullName.Should().Be("Admin Actualizado");
    profile.Data.Phone.Should().Be("8091112222");
    branch!.Data!.Name.Should().Be("Sucursal Actualizada");
    branch.Data.BusinessId.Should().Be(Guid.Parse("11111111-1111-1111-1111-111111111111"));
  }

  [Fact]
  public async Task ChangePasswordShouldFailWhenCurrentPasswordIsInvalid()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);

    var response = await client.PutAsJsonAsync(
      "/api/me/password",
      new ChangeMyPasswordRequest("WrongPassword123!", "NewPassword123!"));
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();

    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeFalse();
    payload.Error!.Code.Should().Be("identity.invalid_current_password");
  }

  [Fact]
  public async Task BusinessUpdateShouldValidatePrimaryPhone()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);

    var response = await client.PutAsJsonAsync(
      "/api/business/current",
      new UpdateCurrentBusinessRequest(
        "Demo Business",
        "Rnc",
        "123456789",
        [
          new RegisterBusinessPhoneRequest("8090000000", "Principal", false)
        ]));
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<CurrentBusinessResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeFalse();
    payload.Error!.Code.Should().Be("validation_error");
  }

  [Fact]
  public async Task BusinessUpdateShouldRejectDuplicatePhones()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);

    var response = await client.PutAsJsonAsync(
      "/api/business/current",
      new UpdateCurrentBusinessRequest(
        "Demo Business",
        "Rnc",
        "123456789",
        [
          new RegisterBusinessPhoneRequest("8090000000", "Principal", true),
          new RegisterBusinessPhoneRequest("809-000-0000", "Secundario", false)
        ]));
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<CurrentBusinessResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeFalse();
    payload.Error!.Code.Should().Be("validation_error");
  }

  [Fact]
  public async Task BusinessUpdateShouldAcceptValidBusinessData()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);

    var response = await client.PutAsJsonAsync(
      "/api/business/current",
      new UpdateCurrentBusinessRequest(
        "Demo Business Actualizado",
        "Cedula",
        "001-1234567-8",
        [
          new RegisterBusinessPhoneRequest("8090000000", "Principal", true),
          new RegisterBusinessPhoneRequest("8290000000", "Secundario", false)
        ]));
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<CurrentBusinessResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeTrue();
    payload.Data!.Name.Should().Be("Demo Business Actualizado");
    payload.Data.IdentificationType.Should().Be("Cedula");
    payload.Data.IdentificationNumber.Should().Be("00112345678");
    payload.Data.Phones.Should().HaveCount(2);
    payload.Data.Phones.Should().ContainSingle(phone => phone.IsPrimary);
  }

  [Fact]
  public async Task RegisterBusinessShouldReturnValidationErrorsForDuplicatePhones()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();

    var response = await client.PostAsJsonAsync(
      "/api/account/register-business",
      new RegisterBusinessRequest(
        "Inderiva",
        "Franklin De Jesus Duran",
        "info@inderiva.com",
        "Admin123!",
        "Cedula",
        "40231756822",
        [
          new RegisterBusinessPhoneRequest("8493564360", "Principal", true),
          new RegisterBusinessPhoneRequest("8493564360", "Secundario", false)
        ],
        "Sucursal principal"));
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<RegisterBusinessResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeFalse();
    payload.Error.Should().NotBeNull();
    payload.Error!.Code.Should().Be("validation_error");
    payload.Error.ValidationErrors.Should().Contain(error =>
      error.Field == "phones" &&
      error.Message == "Phone numbers must not be duplicated.");
  }

  private static async Task AuthenticateAsync(HttpClient client)
  {
    var loginResponse = await client.PostAsJsonAsync(
      "/api/auth/login",
      new LoginRequest("admin@test.com", "Admin123!"));
    var login = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
      "Bearer",
      login!.Data!.AccessToken);
  }

  private static WebApplicationFactory<Program> CreateFactory()
    => new WebApplicationFactory<Program>()
      .WithWebHostBuilder(builder =>
      {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
          configuration.AddInMemoryCollection(new Dictionary<string, string?>
          {
            ["ConnectionStrings:DefaultConnection"] = "",
            ["RabbitMq:UseInMemory"] = "true",
            ["Jwt:Secret"] = "test-secret-with-at-least-32-characters",
            ["Jwt:Issuer"] = "SaasCommerce.Tests",
            ["Jwt:Audience"] = "SaasCommerce.Tests",
            ["Jwt:AccessTokenMinutes"] = "30"
          });
        });
      });
}
