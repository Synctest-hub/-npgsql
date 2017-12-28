// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class TransactionNpgsqlTest : TransactionTestBase<TransactionNpgsqlTest.TransactionNpgsqlFixture>, IDisposable
    {
        public TransactionNpgsqlTest(TransactionNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        [Fact(Skip= "Npgsql batches the inserts, creating an implicit transaction which fails the test (see https://github.com/npgsql/npgsql/issues/1307)")]
        public override void SaveChanges_can_be_used_with_no_transaction() {}

        [Fact(Skip = "Npgsql batches the inserts, creating an implicit transaction which fails the test (see https://github.com/npgsql/npgsql/issues/1307)")]
        public override Task SaveChangesAsync_can_be_used_with_no_transaction() => null;

        public void Dispose()
        {
           TestNpgsqlRetryingExecutionStrategy.Suspended = true;
        }

        protected override DbContext CreateContextWithConnectionString()
        {
            var options = Fixture.AddOptions(
                    new DbContextOptionsBuilder()
                        .UseNpgsql(TestStore.ConnectionString, b => b.ApplyConfiguration().CommandTimeout(NpgsqlTestStore.CommandTimeout)))
                .UseInternalServiceProvider(Fixture.ServiceProvider);

            return new DbContext(options.Options);
        }

        protected override bool SnapshotSupported => true;

        protected override bool DirtyReadsOccur => false;

        public class TransactionNpgsqlFixture : TransactionFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            {
                new NpgsqlDbContextOptionsBuilder(
                        base.AddOptions(builder)
                            .ConfigureWarnings(w => w.Log(RelationalEventId.QueryClientEvaluationWarning)))
                    .MaxBatchSize(1);
                return builder;
            }
        }
    }
}
