using Microsoft.EntityFrameworkCore;

namespace AgentObservatory.Infrastructure.Persistence;

public sealed class ObservatoryDbContext(DbContextOptions<ObservatoryDbContext> options)
    : DbContext(options);
