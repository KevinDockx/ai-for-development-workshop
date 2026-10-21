namespace ClaimHub.Domain.Models;

public sealed record Installment(int Number, DateOnly DueDate, decimal Amount);
