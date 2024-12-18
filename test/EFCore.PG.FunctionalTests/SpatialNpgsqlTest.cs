using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL;

[RequiresPostgis]
public class SpatialNpgsqlTest(SpatialNpgsqlFixture fixture) : SpatialTestBase<SpatialNpgsqlFixture>(fixture)
{
    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    // This test requires DbConnection to be used with the test store, but SpatialNpgsqlFixture must set useConnectionString to true
    // in order to properly set up the NetTopologySuite internally with the data source.
    public override void Mutation_of_tracked_values_does_not_mutate_values_in_store()
    {
    }
}
