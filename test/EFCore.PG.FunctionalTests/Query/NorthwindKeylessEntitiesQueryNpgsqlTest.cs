namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class NorthwindKeylessEntitiesQueryNpgsqlTest : NorthwindKeylessEntitiesQueryRelationalTestBase<
    NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
{
    // ReSharper disable once UnusedParameter.Local
    public NorthwindKeylessEntitiesQueryNpgsqlTest(NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact(Skip = "https://github.com/dotnet/efcore/issues/21627")]
    public override void KeylessEntity_with_nav_defining_query()
        => base.KeylessEntity_with_nav_defining_query();
}