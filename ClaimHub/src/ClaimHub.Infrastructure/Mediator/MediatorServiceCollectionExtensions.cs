using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace ClaimHub.Infrastructure.Mediator;

public static class MediatorServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IMiniMediator"/> and every closed <see cref="IRequestHandler{TRequest,TResponse}"/>
    /// found in the given assemblies. Handlers are scoped so they can take a DbContext.
    /// </summary>
    public static IServiceCollection AddMiniMediator(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddScoped<IMiniMediator, MiniMediator>();

        foreach (var assembly in assemblies.Distinct())
        {
            var handlerTypes = assembly.GetTypes()
                .Where(t => t is { IsAbstract: false, IsInterface: false, IsGenericTypeDefinition: false });

            foreach (var type in handlerTypes)
            {
                foreach (var contract in type.GetInterfaces().Where(IsHandlerContract))
                {
                    services.AddScoped(contract, type);
                }
            }
        }

        return services;
    }

    private static bool IsHandlerContract(Type type) =>
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IRequestHandler<,>);
}
