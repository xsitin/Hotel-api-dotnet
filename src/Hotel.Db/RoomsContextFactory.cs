using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Hotel.Db;

public class RoomsContextFactory : IDesignTimeDbContextFactory<RoomsContext>
{
    private IConfiguration Configuration { get; }

    public RoomsContextFactory()
    {
        Configuration = LoadAppConfiguration();
    }

    public RoomsContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<RoomsContext>();
        builder.UseSqlServer(Configuration.GetConnectionString("MsSqlDb"));
        return new RoomsContext(builder.Options);
    }

    private static IConfigurationRoot LoadAppConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("settings/appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"settings/appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
    }
}
