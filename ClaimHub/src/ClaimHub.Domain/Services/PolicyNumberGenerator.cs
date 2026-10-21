using ClaimHub.Domain.Enums;

namespace ClaimHub.Domain.Services;

public class PolicyNumberGenerator
{
    private readonly IPolicyNumberSequence _sequence;

    public PolicyNumberGenerator(IPolicyNumberSequence sequence)
    {
        _sequence = sequence;
    }

    public string Generate(ProductType product, int year) =>
        Format(product, year, _sequence.Next(product, year));

    public static string Format(ProductType product, int year, int sequence)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(sequence, 1);
        return $"{Prefix(product)}-{year}-{sequence:D6}";
    }

    public static string Prefix(ProductType product) => product switch
    {
        ProductType.Home => "HOM",
        ProductType.Motor => "MOT",
        ProductType.Travel => "TRV",
        _ => throw new ArgumentOutOfRangeException(nameof(product), product, null)
    };

    public static bool TryParse(string? number, out ProductType product, out int year, out int sequence)
    {
        product = default;
        year = 0;
        sequence = 0;

        var parts = number?.Split('-');
        if (parts is not { Length: 3 })
        {
            return false;
        }

        var found = Enum.GetValues<ProductType>().Where(p => Prefix(p) == parts[0]).ToList();
        if (found.Count == 0)
        {
            return false;
        }

        product = found[0];
        return int.TryParse(parts[1], out year) && int.TryParse(parts[2], out sequence);
    }
}
