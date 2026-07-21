using Boardsync.Api.Common.Database;
using FluentValidation;

namespace Boardsync.Api.Common.Extensions;

/// <summary>
/// Extension methods for IServiceCollection to keep Program.cs clean.
/// Each method groups related service registrations together.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the database connection factory as a singleton.
    /// Singleton is correct here because the factory only holds the connection string —
    /// it doesn't hold an open connection. Each call to CreateConnection() creates a new one.
    /// </summary>
    public static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<IDbConnectionFactory>(new DbConnectionFactory(connectionString));
        return services;
    }

    /// <summary>
    /// Registers all FluentValidation validators found in this assembly.
    /// This scans for any class that implements AbstractValidator<T> and registers it in DI.
    /// </summary>
    public static IServiceCollection AddValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<Program>();
        return services;
    }
}
