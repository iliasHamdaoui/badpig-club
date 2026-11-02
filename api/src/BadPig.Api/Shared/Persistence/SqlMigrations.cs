using System.Data.Common;

namespace BadPig.Shared.Persistence;

/// <summary>Small helpers for PostgreSQL (Npgsql): hand-written SQL, no ORM.</summary>
internal static class Sql
{
    /// <summary>Creates an SQL command with its parameters (always parameters: never text pasted into the query).</summary>
    public static DbCommand Cmd(this DbDataSource ds, string sql, params (string Name, object? Value)[] parameters)
    {
        var cmd = ds.CreateCommand(sql);
        foreach (var (name, value) in parameters)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }

        return cmd;
    }
}

/// <summary>
/// Applies the SQL scripts of the Migrations folder at startup (in name order), each one only once, in a transaction.
/// Scripts that were already applied are never modified: to change the database, add a new script (007_..., 008_...).
/// The tables belong to the modules (see each module's part of AppDbContext), but the schema is still created by these scripts;
/// EF Core migrations per module will replace them later.
/// </summary>
public static class SqlMigrations
{
    // Bookkeeping table. Databases created before the English renaming have a "migrations" table with French columns
    // and old script names: they are carried over once (idempotent, runs at each startup).
    private const string Bootstrap = """
        CREATE TABLE IF NOT EXISTS schema_migrations (
            name       text PRIMARY KEY,
            applied_at timestamptz NOT NULL DEFAULT now());

        DO $$
        BEGIN
            IF to_regclass('public.migrations') IS NOT NULL THEN
                INSERT INTO schema_migrations (name, applied_at)
                    SELECT nom, appliquee_le FROM migrations ON CONFLICT DO NOTHING;
                DROP TABLE migrations;
            END IF;
        END $$;

        UPDATE schema_migrations SET name = CASE name
            WHEN '001_journal.sql'          THEN '001_audit_log.sql'
            WHEN '003_adhesions_titres.sql' THEN '003_memberships_and_board_titles.sql'
            WHEN '004_desactivations.sql'   THEN '004_disabled_accounts.sql'
            WHEN '005_titres_libres.sql'    THEN '005_free_form_titles.sql'
            ELSE name END
        WHERE name IN ('001_journal.sql', '003_adhesions_titres.sql', '004_desactivations.sql', '005_titres_libres.sql');
        """;

    public static async Task Apply(DbDataSource ds, ILogger log, CancellationToken ct = default)
    {
        await using (var cmd = ds.Cmd(Bootstrap))
        {
            await cmd.ExecuteNonQueryAsync(ct);
        }

        var assembly = typeof(SqlMigrations).Assembly;
        var scripts = assembly.GetManifestResourceNames()
            .Where(n => n.StartsWith("migrations/", StringComparison.Ordinal) && n.EndsWith(".sql", StringComparison.Ordinal))
            .Order(StringComparer.Ordinal);

        foreach (var resource in scripts)
        {
            var name = resource["migrations/".Length..];

            await using (var exists = ds.Cmd("SELECT 1 FROM schema_migrations WHERE name = @name", ("name", name)))
            {
                if (await exists.ExecuteScalarAsync(ct) is not null)
                    continue;
            }

            using var stream = assembly.GetManifestResourceStream(resource)
                ?? throw new InvalidOperationException($"Script not found: {resource}");
            using var reader = new StreamReader(stream);
            var sql = await reader.ReadToEndAsync(ct);

            await using var connection = await ds.OpenConnectionAsync(ct);
            await using var transaction = await connection.BeginTransactionAsync(ct);

            await using (var apply = connection.CreateCommand())
            {
                apply.Transaction = transaction;
                apply.CommandText = sql;
                await apply.ExecuteNonQueryAsync(ct);
            }

            await using (var record = connection.CreateCommand())
            {
                record.Transaction = transaction;
                record.CommandText = "INSERT INTO schema_migrations (name) VALUES (@name)";
                var p = record.CreateParameter();
                p.ParameterName = "name";
                p.Value = name;
                record.Parameters.Add(p);
                await record.ExecuteNonQueryAsync(ct);
            }

            await transaction.CommitAsync(ct);
            log.LogInformation("Database: script {Name} applied", name);
        }
    }
}
