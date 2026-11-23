using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HvacConnect.Infrastructure.Data;

public sealed class HvacDbContextFactory : IDesignTimeDbContextFactory<HvacDbContext>
{
    public HvacDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<HvacDbContext>();
        optionsBuilder
            .UseNpgsql(
                "Host=localhost;Port=5432;Database=TestDB;Username=postgres;Password=admin",
                options => options.MigrationsHistoryTable("__EFMigrationsHistory", HvacDbContext.SchemaName))
            .UseSnakeCaseNamingConvention();

        return new HvacDbContext(optionsBuilder.Options);
    }
}
