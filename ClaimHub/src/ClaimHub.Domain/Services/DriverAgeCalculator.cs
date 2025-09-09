using ClaimHub.Domain.Entities;

namespace ClaimHub.Domain.Services;

public class DriverAgeCalculator
{
    public int Calculate(DateOnly dateOfBirth, DateOnly on)
    {
        if (dateOfBirth > on)
        {
            throw new ArgumentOutOfRangeException(nameof(dateOfBirth), "The date of birth lies after the reference date.");
        }

        return on.Year - dateOfBirth.Year;
    }

    public int Calculate(MotorRisk risk, DateOnly on) => Calculate(risk.DriverDateOfBirth, on);

    public bool IsYoungDriver(DateOnly dateOfBirth, DateOnly on) => Calculate(dateOfBirth, on) < 25;
}
