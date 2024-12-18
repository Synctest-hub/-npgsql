using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class AdHocPrecompiledQueryNpgsqlTest(ITestOutputHelper testOutputHelper)
    : AdHocPrecompiledQueryRelationalTestBase(testOutputHelper)
{
    protected override bool AlwaysPrintGeneratedSources
        => false;

    public override async Task Index_no_evaluatability()
    {
        await base.Index_no_evaluatability();

        AssertSql(
            """
SELECT j."Id", j."IntList", j."JsonThing"
FROM "JsonEntities" AS j
WHERE j."IntList"[j."Id" + 1] = 2
""");
    }

    public override async Task Index_with_captured_variable()
    {
        await base.Index_with_captured_variable();

        AssertSql(
            """
@__id_0='1'

SELECT j."Id", j."IntList", j."JsonThing"
FROM "JsonEntities" AS j
WHERE j."IntList"[@__id_0 + 1] = 2
""");
    }

    public override async Task JsonScalar()
    {
        await base.JsonScalar();

        AssertSql(
            """
SELECT j."Id", j."IntList", j."JsonThing"
FROM "JsonEntities" AS j
WHERE (j."JsonThing" ->> 'StringProperty') = 'foo'
""");
    }

    public override async Task Materialize_non_public()
    {
        await base.Materialize_non_public();

        AssertSql(
            """
@p0='10' (Nullable = true)
@p1='9' (Nullable = true)
@p2='8' (Nullable = true)

INSERT INTO "NonPublicEntities" ("PrivateAutoProperty", "PrivateProperty", "_privateField")
VALUES (@p0, @p1, @p2)
RETURNING "Id";
""",
            //
            """
SELECT n."Id", n."PrivateAutoProperty", n."PrivateProperty", n."_privateField"
FROM "NonPublicEntities" AS n
LIMIT 2
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;

    protected override PrecompiledQueryTestHelpers PrecompiledQueryTestHelpers
        => NpgsqlPrecompiledQueryTestHelpers.Instance;

    protected override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
    {
        builder = base.AddOptions(builder);

        // TODO: Figure out if there's a nice way to continue using the retrying strategy
        var sqlServerOptionsBuilder = new NpgsqlDbContextOptionsBuilder(builder);
        sqlServerOptionsBuilder.ExecutionStrategy(d => new NonRetryingExecutionStrategy(d));
        return builder;
    }
}
