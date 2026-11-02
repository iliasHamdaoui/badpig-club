using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace BadPig.Api.Tests.Infrastructure;

/// <summary>
/// Starts a PostgreSQL container and the application once for all the tests, and empties the database between two tests.
/// Set BADPIG_TEST_DB_REUSE=true to keep the container between two runs (faster locally), and BADPIG_TEST_DB_PORT /
/// BADPIG_TEST_DB_PASSWORD to choose the host port and the password of the database (to connect to it locally).
/// </summary>
public sealed class ApiFixture : IAsyncLifetime
{
    private static readonly bool ReuseContainer =
        string.Equals(Environment.GetEnvironmentVariable("BADPIG_TEST_DB_REUSE"), "true", StringComparison.OrdinalIgnoreCase);

    private readonly PostgreSqlContainer _dbContainer = CreateDatabaseContainer();

    private Respawner _respawner = default!;
    private NpgsqlConnection _connection = default!;

    public TestApplication Application { get; private set; } = default!;

    public async ValueTask InitializeAsync()
    {
        await _dbContainer.StartAsync();

        // The application applies its own SQL migrations at startup: the first client creation starts it.
        Application = new TestApplication(_dbContainer.GetConnectionString()).Prepare();
        Application.CreateClient().Dispose();

        await InitializeRespawnerAsync();
    }

    /// <summary>Empties every table (the migration bookkeeping table is kept) and resets the fakes.</summary>
    public async Task ResetAsync()
    {
        await _respawner.ResetAsync(_connection);
        Application.Reset();
    }

    public async ValueTask DisposeAsync()
    {
        if (Application is not null)
            await Application.DisposeAsync();

        if (_connection is not null)
            await _connection.DisposeAsync();

        if (!ReuseContainer)
            await _dbContainer.DisposeAsync();
    }

    private async Task InitializeRespawnerAsync()
    {
        _connection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        await _connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            TablesToIgnore = [new Respawn.Graph.Table("schema_migrations")],
        });
    }

    private static PostgreSqlContainer CreateDatabaseContainer()
    {
        var builder = new PostgreSqlBuilder("postgres:17-alpine");

        if (ReuseContainer)
        {
            builder = builder
                .WithReuse(true)
                .WithLabel("reuse-id", "badpig-api-integration-postgres");
        }

        // Optional, to browse the test database from a local tool (DBeaver, psql...): user "postgres", database "postgres".
        var localPassword = Environment.GetEnvironmentVariable("BADPIG_TEST_DB_PASSWORD");
        if (!string.IsNullOrWhiteSpace(localPassword))
            builder = builder.WithPassword(localPassword);

        var localPort = Environment.GetEnvironmentVariable("BADPIG_TEST_DB_PORT");
        if (ushort.TryParse(localPort, out var hostPort))
            builder = builder.WithPortBinding(hostPort, 5432);

        return builder.Build();
    }
}
