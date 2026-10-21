using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Tests.Support;
using FluentAssertions;

namespace ClaimHub.Domain.Tests.Entities;

public class PolicyHolderTests
{
    [Fact]
    public void FullName_Combines_First_And_Last_Name()
    {
        TestData.PolicyHolder().FullName.Should().Be("Els Vermeulen");
    }

    [Fact]
    public void FullName_Is_Trimmed_When_Last_Name_Is_Missing()
    {
        TestData.PolicyHolder(last: "").FullName.Should().Be("Els");
    }

    [Theory]
    [InlineData("2025-06-14", 9)]
    [InlineData("2025-06-15", 10)]
    [InlineData("2010-01-01", 0)]
    public void FullYearsAsCustomer_Counts_Completed_Years(string on, int expected)
    {
        TestData.PolicyHolder().FullYearsAsCustomer(DateOnly.Parse(on)).Should().Be(expected);
    }

    [Fact]
    public void ActivePolicyCount_Ignores_Deleted_And_Inactive_Policies()
    {
        var holder = TestData.PolicyHolder();
        var active = new PolicyBuilder().WithStatus(PolicyStatus.Active).Build();
        var deleted = new PolicyBuilder().WithStatus(PolicyStatus.Active).Build();
        deleted.IsDeleted = true;
        var cancelled = new PolicyBuilder().WithStatus(PolicyStatus.Cancelled).Build();
        holder.Policies.Add(active);
        holder.Policies.Add(deleted);
        holder.Policies.Add(cancelled);

        holder.ActivePolicyCount(new DateOnly(2025, 6, 1)).Should().Be(1);
    }

    [Fact]
    public void SoftDelete_Marks_Policyholder_As_Deleted()
    {
        var holder = TestData.PolicyHolder();
        var at = new DateTimeOffset(2025, 5, 1, 10, 0, 0, TimeSpan.Zero);

        holder.SoftDelete("admin", at);

        holder.IsDeleted.Should().BeTrue();
        holder.DeletedBy.Should().Be("admin");
        holder.DeletedAt.Should().Be(at);
    }
}
