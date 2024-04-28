using DropBear.Codex.StateManagement.StateSnapshots;
using DropBear.Codex.StateManagement.StateSnapshots.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DropBear.Codex.StateManagement.Extensions;

public static class ServiceCollectionExtensions
{
    
    public static IServiceCollection AddStateManagement(this IServiceCollection services)
    {
        services.AddScoped<ISnapshotManagerRegistry, SnapshotManagerRegistry>();
        return services;
    }
}
