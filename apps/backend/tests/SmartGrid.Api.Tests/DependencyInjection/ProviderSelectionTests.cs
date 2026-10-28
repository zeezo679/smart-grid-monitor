using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartGrid.Application.Abstractions.Persistence;
using SmartGrid.Infrastructure;
using SmartGrid.Infrastructure.Repositories;

namespace SmartGrid.Api.Tests.DependencyInjection;

public class ProviderSelectionTests
{
    [Fact]
    public void AddSmartGridInfrastructure_WhenProviderIsMock_ResolvesMockRepository()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            ["DataSource:Provider"] = "Mock"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var services = new ServiceCollection();
        services.AddSmartGridInfrastructure(configuration);

        var provider = services.BuildServiceProvider();
        var repo = provider.GetRequiredService<IDashboardReadRepository>();

        Assert.NotNull(repo);
        Assert.IsType<MockDashboardRepository>(repo);
    }

    [Fact]
    public void AddSmartGridInfrastructure_WhenProviderIsMissing_DefaultsToMockRepository()
    {
        var inMemorySettings = new Dictionary<string, string?>();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var services = new ServiceCollection();
        services.AddSmartGridInfrastructure(configuration);

        var provider = services.BuildServiceProvider();
        var repo = provider.GetRequiredService<IDashboardReadRepository>();

        Assert.NotNull(repo);
        Assert.IsType<MockDashboardRepository>(repo);
    }

    [Fact]
    public void AddSmartGridInfrastructure_WhenProviderIsSnowflakeWithConnectionString_ResolvesSnowflakeRepositoryWithoutConnecting()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            ["DataSource:Provider"] = "Snowflake",
            ["ConnectionStrings:Snowflake"] = "account=myorg-test;user=testuser;password=testpw;db=SMART_GRID;schema=MARTS;warehouse=COMPUTE_WH"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var services = new ServiceCollection();
        services.AddSmartGridInfrastructure(configuration);

        var provider = services.BuildServiceProvider();

        // Must construct SnowflakeDashboardRepository without attempting to open connection
        var repo = provider.GetRequiredService<IDashboardReadRepository>();

        Assert.NotNull(repo);
        Assert.IsType<SnowflakeDashboardRepository>(repo);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddSmartGridInfrastructure_WhenProviderIsSnowflakeWithoutConnectionString_ThrowsInvalidOperationException(string? connStr)
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            ["DataSource:Provider"] = "Snowflake"
        };

        if (connStr != null)
        {
            inMemorySettings["ConnectionStrings:Snowflake"] = connStr;
        }

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var services = new ServiceCollection();

        var ex = Assert.Throws<InvalidOperationException>(
            () => services.AddSmartGridInfrastructure(configuration));

        Assert.Contains("Snowflake connection string is missing or empty", ex.Message);
    }

    [Fact]
    public void AddSmartGridInfrastructure_WhenProviderIsUnsupported_ThrowsInvalidOperationException()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            ["DataSource:Provider"] = "PostgreSQL"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var services = new ServiceCollection();

        var ex = Assert.Throws<InvalidOperationException>(
            () => services.AddSmartGridInfrastructure(configuration));

        Assert.Contains("Unsupported DataSource:Provider 'PostgreSQL'", ex.Message);
    }
}
