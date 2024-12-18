using Microsoft.EntityFrameworkCore.Query.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

// ReSharper disable InconsistentNaming

public class PrecompiledSqlPregenerationQueryNpgsqlTest(
    PrecompiledSqlPregenerationQueryNpgsqlTest.PrecompiledSqlPregenerationQueryNpgsqlFixture fixture,
    ITestOutputHelper testOutputHelper)
    : PrecompiledSqlPregenerationQueryRelationalTestBase(fixture, testOutputHelper),
        IClassFixture<PrecompiledSqlPregenerationQueryNpgsqlTest.PrecompiledSqlPregenerationQueryNpgsqlFixture>
{
    protected override bool AlwaysPrintGeneratedSources
        => false;

    public override async Task No_parameters()
    {
        await base.No_parameters();

        AssertSql(
            """
SELECT b."Id", b."Name"
FROM "Blogs" AS b
WHERE b."Name" = 'foo'
""");
    }

    public override async Task Non_nullable_value_type()
    {
        await base.Non_nullable_value_type();

        AssertSql(
            """
@__id_0='8'

SELECT b."Id", b."Name"
FROM "Blogs" AS b
WHERE b."Id" = @__id_0
""");
    }

    public override async Task Nullable_value_type()
    {
        await base.Nullable_value_type();

        AssertSql(
            """
@__id_0='8' (Nullable = true)

SELECT b."Id", b."Name"
FROM "Blogs" AS b
WHERE b."Id" = @__id_0
""");
    }

    public override async Task Nullable_reference_type()
    {
        await base.Nullable_reference_type();

        AssertSql(
            """
@__name_0='bar'

SELECT b."Id", b."Name"
FROM "Blogs" AS b
WHERE b."Name" = @__name_0
""");
    }

    public override async Task Non_nullable_reference_type()
    {
        await base.Non_nullable_reference_type();

        AssertSql(
            """
@__name_0='bar' (Nullable = false)

SELECT b."Id", b."Name"
FROM "Blogs" AS b
WHERE b."Name" = @__name_0
""");
    }

    public override async Task Nullable_and_non_nullable_value_types()
    {
        await base.Nullable_and_non_nullable_value_types();

        AssertSql(
            """
@__id1_0='8' (Nullable = true)
@__id2_1='9'

SELECT b."Id", b."Name"
FROM "Blogs" AS b
WHERE b."Id" = @__id1_0 OR b."Id" = @__id2_1
""");
    }

    public override async Task Two_nullable_reference_types()
    {
        await base.Two_nullable_reference_types();

        AssertSql(
            """
@__name1_0='foo'
@__name2_1='bar'

SELECT b."Id", b."Name"
FROM "Blogs" AS b
WHERE b."Name" = @__name1_0 OR b."Name" = @__name2_1
""");
    }

    public override async Task Two_non_nullable_reference_types()
    {
        await base.Two_non_nullable_reference_types();

        AssertSql(
            """
@__name1_0='foo' (Nullable = false)
@__name2_1='bar' (Nullable = false)

SELECT b."Id", b."Name"
FROM "Blogs" AS b
WHERE b."Name" = @__name1_0 OR b."Name" = @__name2_1
""");
    }

    public override async Task Nullable_and_non_nullable_reference_types()
    {
        await base.Nullable_and_non_nullable_reference_types();

        AssertSql(
            """
@__name1_0='foo'
@__name2_1='bar' (Nullable = false)

SELECT b."Id", b."Name"
FROM "Blogs" AS b
WHERE b."Name" = @__name1_0 OR b."Name" = @__name2_1
""");
    }

    public override async Task Too_many_nullable_parameters_prevent_pregeneration()
    {
        await base.Too_many_nullable_parameters_prevent_pregeneration();

        AssertSql(
            """
@__name1_0='foo'
@__name2_1='bar'
@__name3_2='baz'
@__name4_3='baq'

SELECT b."Id", b."Name"
FROM "Blogs" AS b
WHERE b."Name" = @__name1_0 OR b."Name" = @__name2_1 OR b."Name" = @__name3_2 OR b."Name" = @__name4_3
""");
    }

    public override async Task Many_non_nullable_parameters_do_not_prevent_pregeneration()
    {
        await base.Many_non_nullable_parameters_do_not_prevent_pregeneration();

        AssertSql(
            """
@__name1_0='foo' (Nullable = false)
@__name2_1='bar' (Nullable = false)
@__name3_2='baz' (Nullable = false)
@__name4_3='baq' (Nullable = false)

SELECT b."Id", b."Name"
FROM "Blogs" AS b
WHERE b."Name" = @__name1_0 OR b."Name" = @__name2_1 OR b."Name" = @__name3_2 OR b."Name" = @__name4_3
""");
    }

    #region Tests for the different querying enumerables

    public override async Task Include_single_query()
    {
        await base.Include_single_query();

        AssertSql(
            """
SELECT b."Id", b."Name", p."Id", p."BlogId", p."Title"
FROM "Blogs" AS b
LEFT JOIN "Post" AS p ON b."Id" = p."BlogId"
ORDER BY b."Id" NULLS FIRST
""");
    }

    public override async Task Include_split_query()
    {
        await base.Include_split_query();

        AssertSql(
            """
SELECT b."Id", b."Name"
FROM "Blogs" AS b
ORDER BY b."Id" NULLS FIRST
""",
            //
            """
SELECT p."Id", p."BlogId", p."Title", b."Id"
FROM "Blogs" AS b
INNER JOIN "Post" AS p ON b."Id" = p."BlogId"
ORDER BY b."Id" NULLS FIRST
""");
    }

    public override async Task Final_GroupBy()
    {
        await base.Final_GroupBy();

        AssertSql(
            """
SELECT b."Name", b."Id"
FROM "Blogs" AS b
ORDER BY b."Name" NULLS FIRST
""");
    }

    #endregion Tests for the different querying enumerables

    public class PrecompiledSqlPregenerationQueryNpgsqlFixture : PrecompiledSqlPregenerationQueryRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        {
            builder = base.AddOptions(builder);

            // TODO: Figure out if there's a nice way to continue using the retrying strategy
            var npgsqlOptionsBuilder = new NpgsqlDbContextOptionsBuilder(builder);
            npgsqlOptionsBuilder
                .ExecutionStrategy(d => new NonRetryingExecutionStrategy(d));
            return builder;
        }

        public override PrecompiledQueryTestHelpers PrecompiledQueryTestHelpers => NpgsqlPrecompiledQueryTestHelpers.Instance;
    }
}
