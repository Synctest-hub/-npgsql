using Microsoft.EntityFrameworkCore.TestModels.FunkyDataModel;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class FunkyDataQueryNpgsqlTest : FunkyDataQueryTestBase<FunkyDataQueryNpgsqlTest.FunkyDataQueryNpgsqlFixture>
{
    // ReSharper disable once UnusedParameter.Local
    public FunkyDataQueryNpgsqlTest(FunkyDataQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override Task String_FirstOrDefault_and_LastOrDefault(bool async)
        => Task.CompletedTask; // Npgsql doesn't support reading an empty string as a char at the ADO level

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task String_starts_with_on_argument_with_escape_constant(bool async)
        => await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.StartsWith("Some\\")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task String_starts_with_on_argument_with_escape_parameter(bool async)
    {
        var param = "Some\\";
        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.StartsWith(param)));
    }

    public class FunkyDataQueryNpgsqlFixture : FunkyDataQueryFixtureBase
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;

        public override FunkyDataContext CreateContext()
        {
            var context = base.CreateContext();
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            return context;
        }

        public override ISetSource GetExpectedData()
            => new NpgsqlFunkyDataData();

        protected override void Seed(FunkyDataContext context)
        {
            context.FunkyCustomers.AddRange(NpgsqlFunkyDataData.CreateFunkyCustomers());
            context.SaveChanges();
        }

        public class NpgsqlFunkyDataData : FunkyDataData
        {
            public new IReadOnlyList<FunkyCustomer> FunkyCustomers { get; }

            public NpgsqlFunkyDataData()
                => FunkyCustomers = CreateFunkyCustomers();

            public override IQueryable<TEntity> Set<TEntity>()
                where TEntity : class
            {
                if (typeof(TEntity) == typeof(FunkyCustomer))
                {
                    return (IQueryable<TEntity>)FunkyCustomers.AsQueryable();
                }

                return base.Set<TEntity>();
            }

            public new static IReadOnlyList<FunkyCustomer> CreateFunkyCustomers()
            {
                var customers = FunkyDataData.CreateFunkyCustomers();
                var maxId = customers.Max(c => c.Id);

                return customers
                    .Append(new FunkyCustomer
                    {
                        Id = maxId + 1,
                        FirstName = "Some\\Guy",
                        LastName = null
                    })
                    .ToList();
            }
        }
    }
}