﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestModels.Array;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable ConvertToConstant.Local

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public abstract class ArrayQueryTest<TFixture> : QueryTestBase<TFixture>
        where TFixture : ArrayQueryFixture, new()
    {
        // ReSharper disable once UnusedParameter.Local
        public ArrayQueryTest(TFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        #region Roundtrip

        [ConditionalFact]
        public void Roundtrip()
        {
            using var ctx = CreateContext();
            var x = ctx.SomeEntities.Single(e => e.Id == 1);

            Assert.Equal(new[] { 3, 4 }, x.IntArray);
            Assert.Equal(new List<int> { 3, 4 }, x.IntList);
            Assert.Equal(new int?[] { 3, 4, null }, x.NullableIntArray);
            Assert.Equal(
                new List<int?>
                {
                    3,
                    4,
                    null
                }, x.NullableIntList);
        }

        #endregion

        #region Indexers

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Index_with_constant(bool async)
            => AssertQuery(
                async,
                ss => ss.Set<ArrayEntity>().Where(e => e.IntArray[0] == 3),
                entryCount: 1);

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Index_with_parameter(bool async)
        {
            // ReSharper disable once ConvertToConstant.Local
            var x = 0;

            return AssertQuery(
                async,
                ss => ss.Set<ArrayEntity>().Where(e => e.IntArray[x] == 3),
                entryCount: 1);
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Nullable_index_with_constant(bool async)
            => AssertQuery(
                async,
                ss => ss.Set<ArrayEntity>().Where(e => e.NullableIntArray[0] == 3),
                entryCount: 1);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Nullable_value_array_index_compare_to_null(bool async)
            => AssertQuery(
                async,
                ss => ss.Set<ArrayEntity>()
                    .Where(e => e.NullableIntArray[2] == null),
                entryCount: 1);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Non_nullable_value_array_index_compare_to_null(bool async)
            => AssertQuery(
                async,
                ss => ss.Set<ArrayEntity>()
#pragma warning disable CS0472
                    .Where(e => e.IntArray[1] == null));
#pragma warning restore CS0472

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Nullable_reference_array_index_compare_to_null(bool async)
            => AssertQuery(
                async,
                ss => ss.Set<ArrayEntity>()
                    .Where(e => e.NullableStringArray[2] == null),
                entryCount: 1);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Non_nullable_reference_array_index_compare_to_null(bool async)
            => AssertQuery(
                async,
                ss => ss.Set<ArrayEntity>()
#pragma warning disable CS0472
                    .Where(e => e.StringArray[1] == null));
#pragma warning restore CS0472

        #endregion Indexers

        #region SequenceEqual

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task SequenceEqual_with_parameter(bool async)
        {
            var arr = new[] { 3, 4 };

            await AssertQuery(
                async,
                ss => ss.Set<ArrayEntity>().Where(e => e.IntArray.SequenceEqual(arr)),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task SequenceEqual_with_array_literal(bool async)
            => await AssertQuery(
                async,
                ss => ss.Set<ArrayEntity>().Where(e => e.IntArray.SequenceEqual(new[] { 3, 4 })),
                entryCount: 1);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task SequenceEqual_over_nullable_with_parameter(bool async)
        {
            var arr = new int?[] { 3, 4, null };

            await AssertQuery(
                async,
                ss => ss.Set<ArrayEntity>().Where(e => e.NullableIntArray.SequenceEqual(arr)),
                entryCount: 1);
        }

        #endregion

        #region Containment

        // See also tests in NorthwindMiscellaneousQueryNpgsqlTest

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Array_column_Any_equality_operator(bool async)
            => await AssertQuery(
                async,
                ss => ss.Set<ArrayEntity>().Where(e => e.StringArray.Any(p => p == "3")),
                entryCount: 1);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Array_column_Any_Equals(bool async)
            => await AssertQuery(
                async,
                ss => ss.Set<ArrayEntity>().Where(e => e.StringArray.Any(p => "3".Equals(p))),
                entryCount: 1);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Array_column_Contains_literal_item(bool async)
            => await AssertQuery(
                async,
                ss => ss.Set<ArrayEntity>().Where(e => e.IntArray.Contains(3)),
                entryCount: 1);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Array_column_Contains_parameter_item(bool async)
        {
            var p = 3;

            await AssertQuery(
                async,
                ss => ss.Set<ArrayEntity>().Where(e => e.IntArray.Contains(p)),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Array_column_Contains_column_item(bool async)
            => await AssertQuery(
                async,
                ss => ss.Set<ArrayEntity>().Where(e => e.IntArray.Contains(e.Id + 2)),
                entryCount: 1);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Array_column_Contains_null_constant(bool async)
            => await AssertQuery(
                async,
                ss => ss.Set<ArrayEntity>().Where(e => e.NullableStringArray.Contains(null)),
                entryCount: 1);

        [ConditionalFact]
        public abstract void Array_column_Contains_null_parameter_does_not_work();

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Nullable_array_column_Contains_literal_item(bool async)
            => await AssertQuery(
                async,
                ss => ss.Set<ArrayEntity>().Where(e => e.NullableIntArray.Contains(3)),
                entryCount: 1);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public abstract Task Array_constant_Contains_column(bool async);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public abstract Task Array_param_Contains_nullable_column(bool async);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public abstract Task Array_param_Contains_non_nullable_column(bool async);

        [ConditionalFact]
        public abstract void Array_param_with_null_Contains_non_nullable_not_found();

        [ConditionalFact]
        public abstract void Array_param_with_null_Contains_non_nullable_not_found_negated();

        [ConditionalFact]
        public abstract void Array_param_with_null_Contains_nullable_not_found();

        [ConditionalFact]
        public abstract void Array_param_with_null_Contains_nullable_not_found_negated();

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public abstract Task Byte_array_parameter_contains_column(bool async);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public abstract Task Array_param_Contains_value_converted_column(bool async);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Array_column_Contains_value_converted_param(bool async)
        {
            var item = SomeEnum.Eight;

            await AssertQuery(
                async,
                ss => ss.Set<ArrayEntity>().Where(e => e.ValueConvertedArray.Contains(item)),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public abstract Task Array_param_Contains_value_converted_array_column(bool async);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Array_column_Contains_in_scalar_subquery(bool async)
            => await AssertQuery(
                async,
                ss => ss.Set<ArrayContainerEntity>().Where(c => c.ArrayEntities.OrderBy(e => e.Id).First().NullableIntArray.Contains(3)),
                entryCount: 1);

        #endregion

        #region Length/Count

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public abstract Task Array_Length(bool async);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public abstract Task Nullable_array_Length(bool async);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public abstract Task Array_Length_on_EF_Property(bool async);

        #endregion Length/Count

        #region Any/All

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Any_no_predicate(bool async)
            => await AssertQuery(
                async,
                ss => ss.Set<ArrayEntity>().Where(e => e.IntArray.Any()),
                entryCount: 2);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public abstract Task Any_like(bool async);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public abstract Task Any_ilike(bool async);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public abstract Task Any_like_anonymous(bool async);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public abstract Task All_like(bool async);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public abstract Task All_ilike(bool async);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Any_Contains_on_constant_array(bool async)
            => await AssertQuery(
                async,
                ss => ss.Set<ArrayEntity>().Where(e => new[] { 2, 3 }.Any(p => e.IntArray.Contains(p))),
                entryCount: 1);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Any_Contains_between_column_and_List(bool async)
        {
            var ints = new List<int> { 2, 3 };

            await AssertQuery(
                async,
                ss => ss.Set<ArrayEntity>().Where(e => e.IntArray.Any(i => ints.Contains(i))),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Any_Contains_between_column_and_array(bool async)
        {
            var ints = new[] { 2, 3 };

            await AssertQuery(
                async,
                ss => ss.Set<ArrayEntity>().Where(e => e.IntArray.Any(i => ints.Contains(i))),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task All_Contains(bool async)
            => await AssertQuery(
                async,
                ss => ss.Set<ArrayEntity>().Where(e => new[] { 5, 6 }.All(p => e.IntArray.Contains(p))),
                entryCount: 1);

        [ConditionalFact]
        public virtual async Task Any_like_column()
        {
            using var ctx = CreateContext();

            await AssertTranslationFailed(
                () => ctx.SomeEntities
                    .Where(e => e.StringArray.Any(p => EF.Functions.Like(p, "3")))
                    .ToListAsync());
        }

        #endregion Any/All

        #region Other translations

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Append(bool async)
            => AssertQuery(
                async,
                ss => ss.Set<ArrayEntity>()
                    .Where(e => e.IntArray.Append(5).SequenceEqual(new[] { 3, 4, 5 })),
                entryCount: 1);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Concat(bool async)
            => AssertQuery(
                async,
                ss => ss.Set<ArrayEntity>()
                    .Where(e => e.IntArray.Concat(new[] { 5, 6 }).SequenceEqual(new[] { 3, 4, 5, 6 })),
                entryCount: 1);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public abstract Task Array_IndexOf1(bool async);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public abstract Task Array_IndexOf2(bool async);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_Join(bool async)
            => AssertQuery(
                async,
                ss => ss.Set<ArrayEntity>()
                    .Where(e => string.Join(", ", e.IntArray) == "3, 4"),
                entryCount: 1);

        #endregion Other translations

        #region Support

        protected ArrayQueryContext CreateContext()
            => Fixture.CreateContext();

        protected void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        #endregion
    }
}
