using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using RPCMAS.Core.Interfaces;
using RPCMAS.Infrastructure.Caching;
using RPCMAS.Infrastructure.Data;
using RPCMAS.Infrastructure.Persistence;
using RPCMAS.Infrastructure.Services;

namespace RPCMAS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("Default")
            ?? throw new InvalidOperationException("ConnectionStrings:Default is missing.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
            {
                sql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
                sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
            }));

        var redisConnection = config["Redis:Connection"] ?? "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(BuildRedisOptions(redisConnection)));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IItemRepository, ItemRepository>();
        services.AddScoped<IPriceChangeRequestRepository, PriceChangeRequestRepository>();
        services.AddScoped<IRequestNumberGenerator, RequestNumberGenerator>();
        services.AddSingleton<ICacheService, RedisCacheService>();

        services.AddScoped<IItemService, ItemService>();
        services.AddScoped<IPriceChangeRequestService, PriceChangeRequestService>();

        return services;
    }

    private static ConfigurationOptions BuildRedisOptions(string connection)
    {
        var opts = ConfigurationOptions.Parse(connection);
        opts.AbortOnConnectFail = false;
        opts.ConnectRetry = 5;
        opts.ConnectTimeout = 5000;
        return opts;
    }
}
