using System.Data.Common;
using ClaimHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ClaimHub.Infrastructure.Services;

/// <summary>
/// Reserves the next value of a named sequence with a single SQLite UPSERT ... RETURNING statement inside a transaction.
/// SQLite holds the database write lock for the statement, so concurrent callers in any process get distinct values.
/// </summary>
internal static class NumberSequenceReservation
{
    private const string ReserveSql =
        "INSERT INTO NumberSequences (Name, LastValue) VALUES ($name, 1) " +
        "ON CONFLICT(Name) DO UPDATE SET LastValue = LastValue + 1 " +
        "RETURNING LastValue;";

    public static async Task<int> ReserveAsync(ClaimHubDbContext context, string name, CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        await using var command = CreateCommand(context, name, transaction.GetDbTransaction());

        int value = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
        await transaction.CommitAsync(cancellationToken);
        return value;
    }

    public static int Reserve(ClaimHubDbContext context, string name)
    {
        using var transaction = context.Database.BeginTransaction();
        using var command = CreateCommand(context, name, transaction.GetDbTransaction());

        int value = Convert.ToInt32(command.ExecuteScalar());
        transaction.Commit();
        return value;
    }

    private static DbCommand CreateCommand(ClaimHubDbContext context, string name, DbTransaction transaction)
    {
        var command = context.Database.GetDbConnection().CreateCommand();
        command.Transaction = transaction;
        command.CommandText = ReserveSql;

        var parameter = command.CreateParameter();
        parameter.ParameterName = "$name";
        parameter.Value = name;
        command.Parameters.Add(parameter);

        return command;
    }
}
