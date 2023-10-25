namespace ClaimHub.Domain.Entities;

public class MotorRisk
{
    public int Id { get; set; }

    public int PolicyId { get; set; }

    public virtual Policy? Policy { get; set; }

    public string LicensePlate { get; set; } = string.Empty;

    public string Make { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public int FirstRegistrationYear { get; set; }

    public decimal VehicleValue { get; set; }

    public int PowerKw { get; set; }

    public DateOnly DriverDateOfBirth { get; set; }

    public int ClaimFreeYears { get; set; }

    public bool HasValidPower() => PowerKw > 0;
}
