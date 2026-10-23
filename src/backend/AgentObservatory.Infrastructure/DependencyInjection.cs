using AgentObservatory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AgentObservatory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<ObservatoryDbContext>(options =>
            options.UseNpgsql(connectionString, postgres => postgres.CommandTimeout(5)));

        return services;
    }
}
