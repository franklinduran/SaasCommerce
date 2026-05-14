using SaasCommerce.Infrastructure.Time;
using FluentAssertions;

namespace SaasCommerce.Infrastructure.Tests;

public sealed class SystemClockTests
{
  [Fact]
  public void UtcNowShouldReturnCurrentUtcTime()
  {
    var before = DateTimeOffset.UtcNow.AddSeconds(-1);

    var now = new SystemClock().UtcNow;

    now.Should().BeAfter(before);
    now.Offset.Should().Be(TimeSpan.Zero);
  }
}
