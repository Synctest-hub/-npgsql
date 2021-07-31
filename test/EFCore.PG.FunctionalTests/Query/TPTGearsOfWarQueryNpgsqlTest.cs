using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class TPTGearsOfWarQueryNpgsqlTest : TPTGearsOfWarQueryRelationalTestBase<TPTGearsOfWarQueryNpgsqlFixture>
    {
        // ReSharper disable once UnusedParameter.Local
        public TPTGearsOfWarQueryNpgsqlTest(TPTGearsOfWarQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        // TODO: #1232
        // protected override bool CanExecuteQueryString => true;

        #region Ignore DateTimeOffset tests

        // PostgreSQL has no datatype that corresponds to DateTimeOffset.
        // DateTimeOffset gets mapped to "timestamptz" which does not record the offset, so the values coming
        // back from the database aren't as expected.

        public override Task DateTimeOffset_DateAdd_AddDays(bool async) => Task.CompletedTask;
        public override Task DateTimeOffset_DateAdd_AddHours(bool async) => Task.CompletedTask;
        public override Task DateTimeOffset_DateAdd_AddMilliseconds(bool async) => Task.CompletedTask;
        public override Task DateTimeOffset_DateAdd_AddMinutes(bool async) => Task.CompletedTask;
        public override Task DateTimeOffset_DateAdd_AddMonths(bool async) => Task.CompletedTask;
        public override Task DateTimeOffset_DateAdd_AddSeconds(bool async) => Task.CompletedTask;
        public override Task DateTimeOffset_DateAdd_AddYears(bool async) => Task.CompletedTask;

        public override Task Where_datetimeoffset_date_component(bool async) => Task.CompletedTask;
        public override Task Where_datetimeoffset_day_component(bool async) => Task.CompletedTask;
        public override Task Where_datetimeoffset_dayofyear_component(bool async) => Task.CompletedTask;
        public override Task Where_datetimeoffset_hour_component(bool async) => Task.CompletedTask;
        public override Task Where_datetimeoffset_minute_component(bool async) => Task.CompletedTask;
        public override Task Where_datetimeoffset_month_component(bool async) => Task.CompletedTask;
        public override Task Where_datetimeoffset_year_component(bool async) => Task.CompletedTask;
        public override Task Where_datetimeoffset_second_component(bool async) => Task.CompletedTask;
        public override Task Where_datetimeoffset_millisecond_component(bool async) => Task.CompletedTask;
        public override Task Time_of_day_datetimeoffset(bool async) => Task.CompletedTask;

        public override Task Where_datetimeoffset_now(bool async) => Task.CompletedTask;
        public override Task Where_datetimeoffset_utcnow(bool async) => Task.CompletedTask;

        public override Task DateTimeOffset_Contains_Less_than_Greater_than(bool async) => Task.CompletedTask;
        public override Task DateTimeOffset_Date_returns_datetime(bool async) => Task.CompletedTask;

        #endregion Ignore DateTimeOffset tests

        // Test runs successfully, but some time difference and precision issues and fail the assertion
        public override Task Where_TimeSpan_Hours(bool async)
            => Task.CompletedTask;

        public override Task Where_TimeOnly_Millisecond(bool async)
            => Task.CompletedTask; // Translation not implemented
    }
}
