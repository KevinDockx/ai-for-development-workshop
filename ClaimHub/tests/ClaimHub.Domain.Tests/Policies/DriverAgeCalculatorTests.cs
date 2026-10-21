using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Services;

namespace ClaimHub.Domain.Tests.Policies;

public class DriverAgeCalculatorTests
{
    private readonly DriverAgeCalculator _calculator = new();

    [Fact]
    public void Should_Calculate_Age_From_Birth_Year()
    {
        var age = _calculator.Calculate(new DateOnly(1990, 1, 1), new DateOnly(2025, 6, 1));

        Assert.Equal(35, age);
    }

    [Fact]
    public void Should_Throw_When_Birth_Date_Is_In_The_Future()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => _calculator.Calculate(new DateOnly(2030, 1, 1), new DateOnly(2025, 6, 1)));
    }

    [Fact]
    public void Should_Use_Date_Of_Birth_From_Motor_Risk()
    {
        var risk = new MotorRisk { DriverDateOfBirth = new DateOnly(2000, 3, 3) };

        Assert.Equal(25, _calculator.Calculate(risk, new DateOnly(2025, 6, 1)));
    }

    [Fact]
    public void Should_Treat_Drivers_Under_25_As_Young()
    {
        Assert.True(_calculator.IsYoungDriver(new DateOnly(2003, 5, 1), new DateOnly(2025, 6, 1)));
        Assert.False(_calculator.IsYoungDriver(new DateOnly(1985, 5, 1), new DateOnly(2025, 6, 1)));
    }
}
