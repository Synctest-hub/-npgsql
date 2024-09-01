﻿using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Migrations
{
    public class MigrationsInfrastructureNpgsqlTest(MigrationsInfrastructureNpgsqlTest.MigrationsInfrastructureNpgsqlFixture fixture)
        : MigrationsInfrastructureTestBase<MigrationsInfrastructureNpgsqlTest.MigrationsInfrastructureNpgsqlFixture>(fixture)
    {
        // TODO: The following test the migration lock, which isn't yet implemented - waiting for EF-side fixes in rc.2
        #region Unskip for 9.0.0-rc.2

        public override void Can_apply_one_migration_in_parallel()
        {
        }

        public override Task Can_apply_one_migration_in_parallel_async()
            => Task.CompletedTask;

        public override void Can_apply_second_migration_in_parallel()
        {
        }

        public override Task Can_apply_second_migration_in_parallel_async()
            => Task.CompletedTask;

        #endregion Unskip for 9.0.0-rc.2

        public override void Can_get_active_provider()
        {
            base.Can_get_active_provider();

            Assert.Equal("Npgsql.EntityFrameworkCore.PostgreSQL", ActiveProvider);
        }

        [ConditionalFact(Skip = "https://github.com/dotnet/efcore/issues/33056")]
        public override void Can_apply_all_migrations()
            => base.Can_apply_all_migrations();

        [ConditionalFact(Skip = "https://github.com/dotnet/efcore/issues/33056")]
        public override void Can_apply_range_of_migrations()
            => base.Can_apply_range_of_migrations();

        [ConditionalFact(Skip = "https://github.com/dotnet/efcore/issues/33056")]
        public override void Can_revert_all_migrations()
            => base.Can_revert_all_migrations();

        [ConditionalFact(Skip = "https://github.com/dotnet/efcore/issues/33056")]
        public override void Can_revert_one_migrations()
            => base.Can_revert_one_migrations();

        [ConditionalFact(Skip = "https://github.com/dotnet/efcore/issues/33056")]
        public override Task Can_apply_all_migrations_async()
            => base.Can_apply_all_migrations_async();

        [ConditionalFact]
        public async Task Empty_Migration_Creates_Database()
        {
            await using var context = new BloggingContext(
                Fixture.TestStore.AddProviderOptions(
                        new DbContextOptionsBuilder().EnableServiceProviderCaching(false))
                    .ConfigureWarnings(e => e.Log(RelationalEventId.PendingModelChangesWarning)).Options);

            var creator = (NpgsqlDatabaseCreator)context.GetService<IRelationalDatabaseCreator>();
            creator.RetryTimeout = TimeSpan.FromMinutes(10);

            await context.Database.MigrateAsync();

            Assert.True(creator.Exists());
        }

        private class BloggingContext(DbContextOptions options) : DbContext(options)
        {
            // ReSharper disable once UnusedMember.Local
            public DbSet<Blog> Blogs { get; set; }

            // ReSharper disable once ClassNeverInstantiated.Local
            public class Blog
            {
                // ReSharper disable UnusedMember.Local
                public int Id { get; set; }

                public string Name { get; set; }
                // ReSharper restore UnusedMember.Local
            }
        }

        [DbContext(typeof(BloggingContext))]
        [Migration("00000000000000_Empty")]
        public class EmptyMigration : Migration
        {
            protected override void Up(MigrationBuilder migrationBuilder)
            {
            }
        }

        public override void Can_diff_against_2_2_model()
        {
            using var context = new ModelSnapshot22.BloggingContext();
            DiffSnapshot(new BloggingContextModelSnapshot22(), context);
        }

        public class BloggingContextModelSnapshot22 : ModelSnapshot
        {
            protected override void BuildModel(ModelBuilder modelBuilder)
            {
#pragma warning disable 612, 618
                modelBuilder
                    .HasAnnotation("ProductVersion", "2.2.4-servicing-10062")
                    .HasAnnotation("Relational:MaxIdentifierLength", 128)
                    .HasAnnotation(
                        "Npgsql:ValueGenerationStrategy",
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                modelBuilder.Entity(
                    "ModelSnapshot22.Blog", b =>
                    {
                        b.Property<int>("Id")
                            .ValueGeneratedOnAdd()
                            .HasAnnotation(
                                "Npgsql:ValueGenerationStrategy",
                                NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                        b.Property<string>("Name");

                        b.HasKey("Id");

                        b.ToTable("Blogs");
                    });

                modelBuilder.Entity(
                    "ModelSnapshot22.Post", b =>
                    {
                        b.Property<int>("Id")
                            .ValueGeneratedOnAdd()
                            .HasAnnotation(
                                "Npgsql:ValueGenerationStrategy",
                                NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                        b.Property<int?>("BlogId");

                        b.Property<string>("Content");

                        b.Property<DateTime>("EditDate");

                        b.Property<string>("Title");

                        b.HasKey("Id");

                        b.HasIndex("BlogId");

                        b.ToTable("Post");
                    });

                modelBuilder.Entity(
                    "ModelSnapshot22.Post", b =>
                    {
                        b.HasOne("ModelSnapshot22.Blog", "Blog")
                            .WithMany("Posts")
                            .HasForeignKey("BlogId");
                    });
#pragma warning restore 612, 618
            }
        }

        public override void Can_diff_against_3_0_ASP_NET_Identity_model()
        {
            // TODO: Implement
        }

        public override void Can_diff_against_2_2_ASP_NET_Identity_model()
        {
            // TODO: Implement
        }

        public override void Can_diff_against_2_1_ASP_NET_Identity_model()
        {
            // TODO: Implement
        }

        public class MigrationsInfrastructureNpgsqlFixture : MigrationsInfrastructureFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory
                => NpgsqlTestStoreFactory.Instance;

            public override MigrationsContext CreateContext()
            {
                var options = AddOptions(
                        TestStore.AddProviderOptions(new DbContextOptionsBuilder())
                            .UseNpgsql(
                                TestStore.ConnectionString, b => b.ApplyConfiguration()
                                    .SetPostgresVersion(TestEnvironment.PostgresVersion)))
                    .UseInternalServiceProvider(ServiceProvider)
                    .Options;
                return new MigrationsContext(options);
            }
        }
    }
}

namespace ModelSnapshot22
{
    public class Blog
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<Post> Posts { get; set; }
    }

    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime EditDate { get; set; }

        public Blog Blog { get; set; }
    }

    public class BloggingContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql(TestEnvironment.DefaultConnection);

        public DbSet<Blog> Blogs { get; set; }
    }
}
