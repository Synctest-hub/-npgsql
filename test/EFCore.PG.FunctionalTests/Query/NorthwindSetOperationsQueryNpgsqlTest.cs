namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class NorthwindSetOperationsQueryNpgsqlTest
    : NorthwindSetOperationsQueryRelationalTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
{
    // ReSharper disable once UnusedParameter.Local
    public NorthwindSetOperationsQueryNpgsqlTest(
        NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        ClearLog();
        //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}