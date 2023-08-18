// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class TPCManyToManyNoTrackingQueryNpgsqlTest : TPCManyToManyNoTrackingQueryRelationalTestBase<TPCManyToManyQueryNpgsqlFixture>
{
    public TPCManyToManyNoTrackingQueryNpgsqlTest(TPCManyToManyQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }
}
