using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Microsoft.Extensions.DependencyInjection;

namespace ClaimHub.Infrastructure.Mediator;

public class MiniMediator : IMiniMediator
{
    private static readonly ConcurrentDictionary<(Type Request, Type Response), HandlerInvoker> Invokers = new();

    private readonly IServiceProvider _serviceProvider;

    public MiniMediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var invoker = Invokers.GetOrAdd((request.GetType(), typeof(TResponse)), key => new HandlerInvoker(key.Request, key.Response));
        object handler = _serviceProvider.GetService(invoker.HandlerType);
        if (handler == null)
        {
            throw new InvalidOperationException(
                $"No handler is registered for request type {request.GetType().Name}.");
        }

        return invoker.InvokeAsync<TResponse>(handler, request, cancellationToken);
    }

    private sealed class HandlerInvoker
    {
        private readonly MethodInfo _method;

        public HandlerInvoker(Type requestType, Type responseType)
        {
            HandlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
            _method = HandlerType.GetMethod(nameof(IRequestHandler<IRequest<object>, object>.HandleAsync));
        }

        public Type HandlerType { get; }

        public Task<TResponse> InvokeAsync<TResponse>(object handler, object request, CancellationToken cancellationToken)
        {
            try
            {
                return (Task<TResponse>)_method.Invoke(handler, new[] { request, cancellationToken });
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}
