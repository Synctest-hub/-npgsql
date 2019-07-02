﻿using System.Data.Common;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class FromSqlQueryNpgsqlTest : FromSqlQueryTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
    {
        public FromSqlQueryNpgsqlTest(NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture)
            : base(fixture)
        {
        }

        [Fact(Skip = "https://github.com/aspnet/EntityFramework/issues/6563")]
        public override void Bad_data_error_handling_invalid_cast() {}
        [Fact(Skip = "https://github.com/aspnet/EntityFramework/issues/6563")]
        public override void Bad_data_error_handling_invalid_cast_projection() {}

        [Fact(Skip="https://github.com/aspnet/EntityFrameworkCore/pull/15423")]
        public override void FromSqlRaw_does_not_parameterize_interpolated_string() {}

        protected override DbParameter CreateDbParameter(string name, object value)
            => new NpgsqlParameter
            {
                ParameterName = name,
                Value = value
            };
    }
}
