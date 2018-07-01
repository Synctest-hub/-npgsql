﻿using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class EnumQueryTest : IClassFixture<EnumQueryTest.EnumFixture>
    {
        #region Tests

        [Fact]
        public void Roundtrip()
        {
            using (var ctx = CreateContext())
            {
                var x = ctx.SomeEntities.Single(e => e.Id == 1);
                Assert.Equal(MappedEnum.Sad, x.MappedEnum);
            }
        }

        [Fact]
        public void Where_with_constant()
        {
            using (var ctx = CreateContext())
            {
                var x = ctx.SomeEntities.Single(e => e.MappedEnum == MappedEnum.Sad);
                Assert.Equal(MappedEnum.Sad, x.MappedEnum);
                AssertContainsInSql("WHERE e.\"MappedEnum\" = 'sad'::mapped_enum");
            }
        }

        [Fact]
        public void Where_with_parameter()
        {
            using (var ctx = CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var sad = MappedEnum.Sad;
                var x = ctx.SomeEntities.Single(e => e.MappedEnum == sad);
                Assert.Equal(MappedEnum.Sad, x.MappedEnum);
                AssertContainsInSql("(DbType = Object)"); // Not very effective but better than nothing
                AssertContainsInSql("WHERE e.\"MappedEnum\" = @__sad_0");
            }
        }

        [Fact]
        public void Where_with_unmapped_enum_parameter_downcasts_are_implicit()
        {
            using (var ctx = CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var sad = UnmappedEnum.Sad;
                var _ = ctx.SomeEntities.Single(e => e.UnmappedEnum == sad);
                AssertContainsInSql("@__sad_0='?' (DbType = Int32)");
                AssertContainsInSql("WHERE e.\"UnmappedEnum\" = @__sad_0");
            }
        }

        [Fact]
        public void Where_with_unmapped_enum_parameter_downcasts_do_not_matter()
        {
            using (var ctx = CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var sad = UnmappedEnum.Sad;
                var _ = ctx.SomeEntities.Single(e => (int)e.UnmappedEnum == (int)sad);
                AssertContainsInSql("@__sad_0='?' (DbType = Int32)");
                AssertContainsInSql("WHERE e.\"UnmappedEnum\" = @__sad_0");
            }
        }

        [Fact]
        public void Where_with_mapped_enum_parameter_downcasts_do_not_matter()
        {
            using (var ctx = CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var sad = MappedEnum.Sad;
                var _ = ctx.SomeEntities.Single(e => (int)e.MappedEnum == (int)sad);
                AssertContainsInSql("@__sad_0='?' (DbType = Object)");
                AssertContainsInSql("WHERE e.\"MappedEnum\" = @__sad_0");
            }
        }

        [Fact]
        public void Where_with_unmapped_enum_parameter_downcast_for_int_comparison_does_matter()
        {
            using (var ctx = CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var sad = UnmappedEnum.Sad;

                var exception = Assert.Throws<InvalidCastException>(() => ctx.SomeEntities.Single(e => e.EnumValue == (int)sad));

                Assert.Equal(
                    "Can't write CLR type Npgsql.EntityFrameworkCore.PostgreSQL.Query.EnumQueryTest+UnmappedEnum with handler type Int32Handler",
                    exception.Message);
            }
        }

        #endregion

        #region Support

        EnumFixture Fixture { get; }

        public EnumQueryTest(EnumFixture fixture)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
        }

        EnumContext CreateContext() => Fixture.CreateContext();

        void AssertContainsInSql(string expected)
            => Assert.Contains(expected, Fixture.TestSqlLoggerFactory.Sql);

        // ReSharper disable once UnusedMember.Local
        void AssertDoesNotContainInSql(string expected)
            => Assert.DoesNotContain(expected, Fixture.TestSqlLoggerFactory.Sql);

        public class EnumContext : DbContext
        {
            public DbSet<SomeEnumEntity> SomeEntities { get; set; }

            public EnumContext(DbContextOptions options) : base(options) {}

            protected override void OnModelCreating(ModelBuilder builder)
            {
                builder.ForNpgsqlHasEnum("mapped_enum", new[] { "happy", "sad" });
            }
        }

        public class SomeEnumEntity
        {
            public int Id { get; set; }

            public MappedEnum MappedEnum { get; set; }

            public UnmappedEnum UnmappedEnum { get; set; }

            public int EnumValue { get; set; }
        }

        public enum MappedEnum
        {
            // ReSharper disable once UnusedMember.Global
            Happy,
            Sad
        };

        public enum UnmappedEnum
        {
            // ReSharper disable once UnusedMember.Global
            Happy,
            Sad
        };

        public class EnumFixture : IDisposable
        {
            readonly DbContextOptions _options;

            readonly NpgsqlTestStore _testStore;

            public TestSqlLoggerFactory TestSqlLoggerFactory { get; } = new TestSqlLoggerFactory();

            public EnumFixture()
            {
                NpgsqlConnection.GlobalTypeMapper.MapEnum<MappedEnum>();

                _testStore = NpgsqlTestStore.CreateScratch();

                _options = new DbContextOptionsBuilder()
                           .UseNpgsql(_testStore.ConnectionString, b => b.ApplyConfiguration())
                           .UseInternalServiceProvider(
                               new ServiceCollection()
                                   .AddEntityFrameworkNpgsql()
                                   .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                                   .BuildServiceProvider())
                           .Options;

                using (var ctx = CreateContext())
                {
                    ctx.Database.EnsureCreated();

                    ctx.SomeEntities
                       .Add(
                           new SomeEnumEntity
                           {
                               Id = 1,
                               MappedEnum = MappedEnum.Sad,
                               UnmappedEnum = UnmappedEnum.Sad,
                               EnumValue = (int)MappedEnum.Sad
                           });

                    ctx.SaveChanges();
                }
            }

            public EnumContext CreateContext() => new EnumContext(_options);

            public void Dispose() => _testStore.Dispose();
        }

        #endregion
    }
}
