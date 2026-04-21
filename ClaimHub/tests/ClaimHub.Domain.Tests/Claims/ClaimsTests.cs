using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Rules;
using ClaimHub.Domain.Services;
using ClaimHub.Domain.Tests.Support;

namespace ClaimHub.Domain.Tests.Claims;

public class ClaimsTests
{
    [Fact]
    public void Should_Allow_Assessment_After_Reporting()
    {
        var machine = new ClaimStateMachine();

        Assert.True(machine.CanTransition(ClaimStatus.Reported, ClaimStatus.UnderAssessment));
        Assert.False(machine.CanTransition(ClaimStatus.Reported, ClaimStatus.PaidOut));
    }

    [Fact]
    public async Task Should_Request_Number_From_Sequence()
    {
        var sequence = new FakeClaimNumberSequence();
        var generator = new ClaimNumberGenerator(sequence);

        await generator.GenerateAsync(2025);

        Assert.Equal(1, sequence.CallCount);
        Assert.Equal(2025, sequence.LastYear);
    }
}
