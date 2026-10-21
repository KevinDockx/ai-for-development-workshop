namespace ClaimHub.Infrastructure.Mediator;

/// <summary>Marker for a command or query that produces a <typeparamref name="TResponse"/>.</summary>
public interface IRequest<TResponse>
{
}

public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken);
}

public interface IMiniMediator
{
    Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}

/// <summary>Response type for requests that return nothing.</summary>
public readonly struct Unit
{
    public static readonly Unit Value = new();

    public static Task<Unit> AsTask() => Task.FromResult(Value);
}
