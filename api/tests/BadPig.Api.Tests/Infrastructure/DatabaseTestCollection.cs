namespace BadPig.Api.Tests.Infrastructure;

/// <summary>All the integration tests share one PostgreSQL container and one in-memory application (they run one after the other).</summary>
[CollectionDefinition(nameof(DatabaseTestCollection))]
public class DatabaseTestCollection : ICollectionFixture<ApiFixture>;
