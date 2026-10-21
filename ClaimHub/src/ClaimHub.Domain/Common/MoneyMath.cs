namespace ClaimHub.Domain.Common;

public static class MoneyMath
{
    public const MidpointRounding Rounding = MidpointRounding.AwayFromZero;

    public static decimal ToCents(decimal amount) => Math.Round(amount, 2, Rounding);

    public static decimal ToCents(double amount) => Math.Round((decimal)amount, 2, Rounding);

    public static decimal Max(decimal left, decimal right) => left > right ? left : right;

    public static decimal ClampToZero(decimal amount) => amount < 0m ? 0m : amount;
}
