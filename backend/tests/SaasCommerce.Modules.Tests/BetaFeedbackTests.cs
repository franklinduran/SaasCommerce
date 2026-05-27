#pragma warning disable CA1707

using FluentAssertions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Feedback.Application;
using SaasCommerce.Modules.Feedback.Application.Abstractions;
using SaasCommerce.Modules.Feedback.Domain;
using SaasCommerce.Modules.Feedback.Infrastructure.Persistence;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tests;

public sealed class BetaFeedbackTests
{
  private static readonly DateTimeOffset Now = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

  [Fact]
  public void BetaFeedback_ShouldCreateNewFeedback_WhenDataIsValid()
  {
    var businessId = new BusinessId(Guid.NewGuid());
    var userId = Guid.NewGuid();

    var feedback = BetaFeedback.Create(new BetaFeedbackDraft(
      Guid.NewGuid(),
      businessId,
      userId,
      BetaFeedbackCategory.Bug,
      "Error en venta",
      "La venta quedo en Processing.",
      "/sales/1",
      Now));

    feedback.BusinessId.Should().Be(businessId);
    feedback.UserId.Should().Be(userId);
    feedback.Status.Should().Be(BetaFeedbackStatus.New);
    feedback.Title.Should().Be("Error en venta");
  }

  [Fact]
  public void BetaFeedback_ShouldTrimOptionalValues_WhenCreatedAndReviewed()
  {
    var feedback = BetaFeedback.Create(new BetaFeedbackDraft(
      Guid.NewGuid(),
      new BusinessId(Guid.NewGuid()),
      Guid.NewGuid(),
      BetaFeedbackCategory.Question,
      "  Duda de caja  ",
      "  No veo el cierre diario.  ",
      "  /cash-register  ",
      Now));

    feedback.ChangeStatus(BetaFeedbackStatus.Resolved, Guid.NewGuid(), Now.AddMinutes(1), "  Cerrado  ");

    feedback.Title.Should().Be("Duda de caja");
    feedback.Description.Should().Be("No veo el cierre diario.");
    feedback.ContextUrl.Should().Be("/cash-register");
    feedback.ReviewNote.Should().Be("Cerrado");
  }

  [Theory]
  [InlineData("empty-id")]
  [InlineData("empty-user")]
  [InlineData("empty-title")]
  [InlineData("empty-description")]
  public void BetaFeedback_ShouldRejectInvalidDraft(string caseName)
  {
    var draft = new BetaFeedbackDraft(
      caseName == "empty-id" ? Guid.Empty : Guid.NewGuid(),
      new BusinessId(Guid.NewGuid()),
      caseName == "empty-user" ? Guid.Empty : Guid.NewGuid(),
      BetaFeedbackCategory.Bug,
      caseName == "empty-title" ? "" : "Titulo",
      caseName == "empty-description" ? "" : "Descripcion",
      null,
      Now);

    var action = () => BetaFeedback.Create(draft);

    action.Should().Throw<ArgumentException>();
  }

  [Fact]
  public void BetaFeedback_ShouldRejectEmptyReviewer_WhenChangingStatus()
  {
    var feedback = CreateFeedback(new BusinessId(Guid.NewGuid()));

    var action = () => feedback.ChangeStatus(BetaFeedbackStatus.Resolved, Guid.Empty, Now, null);

    action.Should().Throw<ArgumentException>();
  }

  [Fact]
  public void BetaFeedback_ShouldChangeStatus_WhenReviewerIsValid()
  {
    var reviewerId = Guid.NewGuid();
    var feedback = CreateFeedback(new BusinessId(Guid.NewGuid()));

    feedback.ChangeStatus(BetaFeedbackStatus.InReview, reviewerId, Now.AddMinutes(5), "Lo revisamos");

    feedback.Status.Should().Be(BetaFeedbackStatus.InReview);
    feedback.ReviewedByUserId.Should().Be(reviewerId);
    feedback.ReviewNote.Should().Be("Lo revisamos");
  }

  [Theory]
  [InlineData("")]
  [InlineData("Unknown")]
  public async Task CreateBetaFeedbackHandler_ShouldRejectInvalidCategory(string category)
  {
    var repo = new FakeFeedbackRepository();
    var handler = CreateHandler(repo, Authed());

    var result = await handler.Handle(new CreateBetaFeedbackCommand(category, "Titulo", "Descripcion", null));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(BetaFeedbackErrors.InvalidFeedback);
    repo.Items.Should().BeEmpty();
  }

  [Fact]
  public async Task CreateBetaFeedbackHandler_ShouldUseAuthenticatedBusinessId()
  {
    var businessId = Guid.NewGuid();
    var repo = new FakeFeedbackRepository();
    var handler = CreateHandler(repo, Authed(businessId));

    var result = await handler.Handle(new CreateBetaFeedbackCommand(
      "SaleIssue",
      "Venta no completo",
      "La venta se quedo procesando.",
      "/sales/123"));

    result.IsSuccess.Should().BeTrue();
    repo.Items.Should().ContainSingle(item => item.BusinessId.Value == businessId);
    result.Value.BusinessId.Should().Be(businessId);
  }

  [Fact]
  public async Task GetBetaFeedbackHandler_ShouldRejectInvalidFilters()
  {
    var handler = new GetBetaFeedbackHandler(new FakeFeedbackRepository(), Authed());

    var result = await handler.Handle(new GetBetaFeedbackQuery("Missing", "Bug", 0, 25));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(BetaFeedbackErrors.InvalidFeedback);
  }

  [Fact]
  public async Task GetBetaFeedbackHandler_ShouldReturnPagedFilteredResults()
  {
    var businessId = Guid.NewGuid();
    var repo = new FakeFeedbackRepository();
    var accepted = CreateFeedback(new BusinessId(businessId), BetaFeedbackCategory.CashIssue);
    accepted.ChangeStatus(BetaFeedbackStatus.Accepted, Guid.NewGuid(), Now.AddMinutes(1), null);
    repo.Items.Add(accepted);
    repo.Items.Add(CreateFeedback(new BusinessId(businessId), BetaFeedbackCategory.Bug));
    var handler = new GetBetaFeedbackHandler(repo, Authed(businessId));

    var result = await handler.Handle(new GetBetaFeedbackQuery("Accepted", "CashIssue", 1, 10));

    result.IsSuccess.Should().BeTrue();
    result.Value.TotalItems.Should().Be(1);
    result.Value.Items.Should().ContainSingle(item => item.Id == accepted.Id);
    result.Value.HasNextPage.Should().BeFalse();
  }

  [Fact]
  public async Task UpdateBetaFeedbackStatusHandler_ShouldChangeStatus()
  {
    var businessId = Guid.NewGuid();
    var repo = new FakeFeedbackRepository();
    var feedback = CreateFeedback(new BusinessId(businessId));
    repo.Items.Add(feedback);
    var handler = new UpdateBetaFeedbackStatusHandler(
      repo,
      Authed(businessId),
      new FixedClock(),
      new NoopUow(),
      new UpdateBetaFeedbackStatusValidator());

    var result = await handler.Handle(new UpdateBetaFeedbackStatusCommand(
      feedback.Id,
      "Accepted",
      "Prioridad media"));

    result.IsSuccess.Should().BeTrue();
    feedback.Status.Should().Be(BetaFeedbackStatus.Accepted);
    feedback.ReviewNote.Should().Be("Prioridad media");
  }

  [Fact]
  public async Task EfBetaFeedbackRepository_ShouldFilterByBusinessId()
  {
    var options = new DbContextOptionsBuilder<AppDbContext>()
      .UseInMemoryDatabase(Guid.NewGuid().ToString())
      .Options;
    await using var db = new AppDbContext(options);
    var businessId = new BusinessId(Guid.NewGuid());
    var otherBusinessId = new BusinessId(Guid.NewGuid());
    var expected = CreateFeedback(businessId);
    db.Add(expected);
    db.Add(CreateFeedback(otherBusinessId));
    await db.SaveChangesAsync();
    var repo = new EfBetaFeedbackRepository(db);

    var items = await repo.ListAsync(
      businessId,
      new BetaFeedbackSearchCriteria(null, null, 1, 20));
    var hidden = await repo.GetAsync(businessId, db.Set<BetaFeedback>().Single(f => f.BusinessId == otherBusinessId).Id);

    items.Should().ContainSingle(item => item.Id == expected.Id);
    hidden.Should().BeNull();
  }

  [Fact]
  public async Task EfBetaFeedbackRepository_ShouldFilterByStatusAndCategory()
  {
    var options = new DbContextOptionsBuilder<AppDbContext>()
      .UseInMemoryDatabase(Guid.NewGuid().ToString())
      .Options;
    await using var db = new AppDbContext(options);
    var businessId = new BusinessId(Guid.NewGuid());
    var cashIssue = CreateFeedback(businessId, BetaFeedbackCategory.CashIssue);
    cashIssue.ChangeStatus(BetaFeedbackStatus.Resolved, Guid.NewGuid(), Now.AddMinutes(1), null);
    db.Add(cashIssue);
    db.Add(CreateFeedback(businessId, BetaFeedbackCategory.Bug));
    await db.SaveChangesAsync();
    var repo = new EfBetaFeedbackRepository(db);
    var criteria = new BetaFeedbackSearchCriteria(
      BetaFeedbackStatus.Resolved,
      BetaFeedbackCategory.CashIssue,
      1,
      10);

    var items = await repo.ListAsync(businessId, criteria);
    var count = await repo.CountAsync(businessId, criteria);

    items.Should().ContainSingle(item => item.Id == cashIssue.Id);
    count.Should().Be(1);
  }

  private static CreateBetaFeedbackHandler CreateHandler(
    FakeFeedbackRepository repo,
    ICurrentUserService user)
    => new(repo, user, new FixedClock(), new NoopUow(), new CreateBetaFeedbackValidator());

  private static BetaFeedback CreateFeedback(
    BusinessId businessId,
    BetaFeedbackCategory category = BetaFeedbackCategory.Bug)
    => BetaFeedback.Create(new BetaFeedbackDraft(
      Guid.NewGuid(),
      businessId,
      Guid.NewGuid(),
      category,
      "Error de prueba",
      "Detalle de prueba",
      null,
      Now));

  private static FakeUser Authed(Guid? businessId = null)
    => new()
    {
      BusinessId = businessId ?? Guid.NewGuid(),
      UserId = Guid.NewGuid(),
      IsAuthenticated = true,
    };

  private sealed class FakeUser : ICurrentUserService
  {
    public Guid? UserId { get; init; }
    public Guid? BusinessId { get; init; }
    public Guid? BranchId { get; init; }
    public IReadOnlyCollection<string> Roles { get; init; } = [];
    public bool IsAuthenticated { get; init; }
  }

  private sealed class FixedClock : IClock
  {
    public DateTimeOffset UtcNow => Now;
  }

  private sealed class NoopUow : IUnitOfWork
  {
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
      => Task.FromResult(1);
  }

  private sealed class FakeFeedbackRepository : IBetaFeedbackRepository
  {
    public List<BetaFeedback> Items { get; } = [];

    public Task AddAsync(BetaFeedback feedback, CancellationToken cancellationToken = default)
    {
      Items.Add(feedback);
      return Task.CompletedTask;
    }

    public Task<BetaFeedback?> GetAsync(
      BusinessId businessId,
      Guid feedbackId,
      CancellationToken cancellationToken = default)
      => Task.FromResult(Items.SingleOrDefault(item => item.BusinessId == businessId && item.Id == feedbackId));

    public Task<IReadOnlyCollection<BetaFeedback>> ListAsync(
      BusinessId businessId,
      BetaFeedbackSearchCriteria criteria,
      CancellationToken cancellationToken = default)
      => Task.FromResult<IReadOnlyCollection<BetaFeedback>>(
        ApplyCriteria(businessId, criteria).ToArray());

    public Task<int> CountAsync(
      BusinessId businessId,
      BetaFeedbackSearchCriteria criteria,
      CancellationToken cancellationToken = default)
      => Task.FromResult(ApplyCriteria(businessId, criteria).Count());

    private IEnumerable<BetaFeedback> ApplyCriteria(
      BusinessId businessId,
      BetaFeedbackSearchCriteria criteria)
    {
      var query = Items.Where(item => item.BusinessId == businessId);

      if (criteria.Status.HasValue)
      {
        query = query.Where(item => item.Status == criteria.Status.Value);
      }

      if (criteria.Category.HasValue)
      {
        query = query.Where(item => item.Category == criteria.Category.Value);
      }

      return query;
    }
  }
}

#pragma warning restore CA1707
