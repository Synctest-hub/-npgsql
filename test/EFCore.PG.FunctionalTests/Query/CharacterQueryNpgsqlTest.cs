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
    public class CharacterQueryNpgsqlTest : IClassFixture<CharacterQueryNpgsqlTest.CharacterQueryNpgsqlFixture>
    {
        #region Tests

        [Fact]
        public void Find_in_database()
        {
            Fixture.ClearEntities();

            // important: add here so they aren't locally available below.
            using (var ctx = Fixture.CreateContext())
            {
                ctx.CharacterTestEntities.Add(new CharacterTestEntity { Character8 = "12345678" });
                ctx.CharacterTestEntities.Add(new CharacterTestEntity { Character8 = "123456  " });
                ctx.SaveChanges();
            }

            using (var ctx = Fixture.CreateContext())
            {
                const string update = "update";

                var m1 = ctx.CharacterTestEntities.Find("12345678");
                Assert.NotNull(m1);
                m1.Character6 = update;
                ctx.SaveChanges();

                var m2 = ctx.CharacterTestEntities.Find("123456  ");
                Assert.NotNull(m2);
                m2.Character6 = update;
                ctx.SaveChanges();

                var item0 = ctx.CharacterTestEntities.Find("12345678").Character6;
                Assert.Equal(update, item0);

                var item1 = ctx.CharacterTestEntities.Find("123456  ").Character6;
                Assert.Equal(update, item1);
            }
        }

        [Fact]
        public void Find_locally_available()
        {
            Fixture.ClearEntities();

            // important: add here so they are locally available below.
            using (var ctx = Fixture.CreateContext())
            {
                ctx.CharacterTestEntities.Add(new CharacterTestEntity { Character8 = "12345678" });
                ctx.CharacterTestEntities.Add(new CharacterTestEntity { Character8 = "123456  " });
                ctx.SaveChanges();

                const string update = "update";

                var m1 = ctx.CharacterTestEntities.Find("12345678");
                m1.Character6 = update;
                ctx.SaveChanges();

                var m2 = ctx.CharacterTestEntities.Find("123456  ");
                m2.Character6 = update;
                ctx.SaveChanges();

                var item0 = ctx.CharacterTestEntities.Find("12345678").Character6;
                Assert.Equal(update, item0);

                var item1 = ctx.CharacterTestEntities.Find("123456  ").Character6;
                Assert.Equal(update, item1);
            }
        }

        /// <summary>
        /// Test something like: select '123456  '::char(8) = '123456'::char(8);
        /// </summary>
        [Fact]
        public void Test_change_tracking()
        {
            Fixture.ClearEntities();

            using (var ctx = Fixture.CreateContext())
            {
                const string update = "update";

                ctx.CharacterTestEntities.Add(new CharacterTestEntity { Character8 = "12345678" });
                ctx.CharacterTestEntities.Add(new CharacterTestEntity { Character8 = "123456  " });
                ctx.SaveChanges();

                var m1 = ctx.CharacterTestEntities.Find("12345678");
                m1.Character6 = update;
                ctx.SaveChanges();

                var m2 = ctx.CharacterTestEntities.Find("123456  ");
                m2.Character6 = update;
                ctx.SaveChanges();
            }
        }

        /// <summary>
        /// Test that comparisons are treated correctly.
        /// </summary>
        [Fact]
        public void Test_change_tracking_key_sizes()
        {
            Fixture.ClearEntities();

            using (var ctx = Fixture.CreateContext())
            {
                var entity = new CharacterTestEntity { Character8 = "123456  ", Character6 = "12345 " };
                ctx.CharacterTestEntities.Add(entity);
                ctx.SaveChanges();

                var update = ctx.CharacterTestEntities.Single(x => x.Character8 == "123456");
                update.Character6 = entity.Character6.TrimEnd();
                Assert.Equal(1, ctx.SaveChanges());

                var test = ctx.CharacterTestEntities.Single(x => x.Character6 == "12345 ");
                Assert.Equal("12345 ", test.Character6);
            }
        }

        #endregion

        #region Support

        /// <summary>
        /// Provides resources for unit tests.
        /// </summary>
        CharacterQueryNpgsqlFixture Fixture { get; }

        /// <summary>
        /// Initializes resources for unit tests.
        /// </summary>
        /// <param name="fixture">The fixture of resources for testing.</param>
        public CharacterQueryNpgsqlTest(CharacterQueryNpgsqlFixture fixture)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
        }

        #endregion

        #region Fixture

        // ReSharper disable once ClassNeverInstantiated.Global
        /// <summary>
        /// Represents a fixture suitable for testing character data.
        /// </summary>
        public class CharacterQueryNpgsqlFixture : IDisposable
        {
            /// <summary>
            /// The <see cref="NpgsqlTestStore"/> used for testing.
            /// </summary>
            private readonly NpgsqlTestStore _testStore;

            /// <summary>
            /// The <see cref="DbContextOptions"/> used for testing.
            /// </summary>
            private readonly DbContextOptions _options;

            /// <summary>
            /// The logger factory used for testing.
            /// </summary>
            public TestSqlLoggerFactory TestSqlLoggerFactory { get; }

            /// <summary>
            /// Initializes a <see cref="CharacterQueryNpgsqlTest.CharacterQueryNpgsqlFixture"/>.
            /// </summary>
            // ReSharper disable once UnusedMember.Global
            public CharacterQueryNpgsqlFixture()
            {
                TestSqlLoggerFactory = new TestSqlLoggerFactory();

                _testStore = NpgsqlTestStore.CreateScratch();

                _options =
                    new DbContextOptionsBuilder()
                        .UseNpgsql(_testStore.ConnectionString, b => b.ApplyConfiguration())
                        .UseInternalServiceProvider(
                            new ServiceCollection()
                                .AddEntityFrameworkNpgsql()
                                .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                                .BuildServiceProvider())
                        .Options;

                using (var context = CreateContext())
                {
                    context.Database.EnsureCreated();
                }
            }

            /// <summary>
            /// Creates a new <see cref="CharacterContext"/>.
            /// </summary>
            /// <returns>
            /// A <see cref="CharacterContext"/> for testing.
            /// </returns>
            public CharacterContext CreateContext() => new CharacterContext(_options);

            /// <summary>
            /// Clears the entities in the context.
            /// </summary>
            public void ClearEntities()
            {
                using (var ctx = CreateContext())
                {
                    var entities = ctx.CharacterTestEntities.ToArray();

                    foreach (var e in entities)
                        ctx.CharacterTestEntities.Remove(e);

                    ctx.SaveChanges();
                }
            }

            /// <inheritdoc />
            public void Dispose() => _testStore.Dispose();
        }

        public class CharacterTestEntity
        {
            public string Character8 { get; set; }
            public string Character6 { get; set; }
        }

        public class CharacterContext : DbContext
        {
            public DbSet<CharacterTestEntity> CharacterTestEntities { get; set; }

            /// <summary>
            /// Initializes a <see cref="CharacterContext"/>.
            /// </summary>
            /// <param name="options">
            /// The options to be used for configuration.
            /// </param>
            public CharacterContext(DbContextOptions options) : base(options) {}

            /// <inheritdoc />
            protected override void OnModelCreating(ModelBuilder builder)
                => builder.Entity<CharacterTestEntity>(
                    entity =>
                    {
                        entity.HasKey(e => e.Character8);
                        entity.Property(e => e.Character8).HasColumnType("character(8)");
                        entity.Property(e => e.Character6).HasColumnType("character(6)");
                    });
        }

        #endregion

        #region Helpers

        // ReSharper disable once UnusedMember.Global
        /// <summary>
        /// Asserts that the SQL fragment appears in the logs.
        /// </summary>
        /// <param name="sql">The SQL statement or fragment to search for in the logs.</param>
        public void AssertContainsSql(string sql) => Assert.Contains(sql, Fixture.TestSqlLoggerFactory.Sql);

        #endregion
    }
}
