﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class MigrationsInfrastructureNpgsqlTest
        : MigrationsInfrastructureTestBase<MigrationsInfrastructureNpgsqlTest.MigrationsInfrastructureNpgsqlFixture>
    {
        public MigrationsInfrastructureNpgsqlTest(MigrationsInfrastructureNpgsqlFixture fixture)
            : base(fixture) {}

        public override void Can_get_active_provider()
        {
            base.Can_get_active_provider();

            Assert.Equal("Npgsql.EntityFrameworkCore.PostgreSQL", ActiveProvider);
        }

        static string GetDatabaseSchemaAsync(DbConnection connection)
        {
            var builder = new IndentedStringBuilder();
            var command = connection.CreateCommand();
            command.CommandText = @"
SELECT table_name,
	column_name,
	udt_name,
	is_nullable = 'YES',
	column_default
FROM information_schema.columns
WHERE table_catalog = @db
	AND table_schema = 'public'
ORDER BY table_name, ordinal_position
";

            var dbName = connection.Database;
            command.Parameters.Add(new NpgsqlParameter { ParameterName = "db", Value = dbName });

            using var reader = command.ExecuteReader();
            var first = true;
            string lastTable = null;
            while (reader.Read())
            {
                var currentTable = reader.GetString(0);
                if (currentTable != lastTable)
                {
                    if (first)
                        first = false;
                    else
                        builder.DecrementIndent();

                    builder
                        .AppendLine()
                        .AppendLine(currentTable)
                        .IncrementIndent();

                    lastTable = currentTable;
                }

                builder
                    .Append(reader[1]) // Name
                    .Append(" ")
                    .Append(reader[2]) // Type
                    .Append(" ")
                    .Append(reader.GetBoolean(3) ? "NULL" : "NOT NULL");

                if (!reader.IsDBNull(4))
                {
                    builder
                        .Append(" DEFAULT ")
                        .Append(reader[4]);
                }

                builder.AppendLine();
            }

            return builder.ToString();
        }

        [ConditionalFact]
        public async Task Empty_Migration_Creates_Database()
        {
            using var context = new BloggingContext(
                Fixture.TestStore.AddProviderOptions(
                    new DbContextOptionsBuilder().EnableServiceProviderCaching(false)).Options);

            var creator = (NpgsqlDatabaseCreator)context.GetService<IRelationalDatabaseCreator>();
            creator.RetryTimeout = TimeSpan.FromMinutes(10);

            await context.Database.MigrateAsync();

            Assert.True(creator.Exists());
        }

        class BloggingContext : DbContext
        {
            public BloggingContext(DbContextOptions options)
                : base(options)
            {
            }

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
//            using (var context = new ModelSnapshot22.BloggingContext())
//            {
//                DiffSnapshot(new BloggingContextModelSnapshot22(), context);
//            }
//
            using var context = new ModelSnapshot22.BloggingContext();
            var snapshot = new BloggingContextModelSnapshot22();
            var sourceModel = snapshot.Model;
            var targetModel = context.Model;

            var modelDiffer = context.GetService<IMigrationsModelDiffer>();
            var operations = modelDiffer.GetDifferences(sourceModel, targetModel);

            Assert.Equal(0, operations.Count);
        }

        public class BloggingContextModelSnapshot22 : ModelSnapshot
        {
            protected override void BuildModel(ModelBuilder modelBuilder)
            {
#pragma warning disable 612, 618
                modelBuilder
                    .HasAnnotation("ProductVersion", "2.2.4-servicing-10062")
                    .HasAnnotation("Relational:MaxIdentifierLength", 128)
                    .HasAnnotation("Npgsql:ValueGenerationStrategy",
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                modelBuilder.Entity("ModelSnapshot22.Blog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Blogs");
                });

                modelBuilder.Entity("ModelSnapshot22.Post", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int?>("BlogId");

                    b.Property<string>("Content");

                    b.Property<DateTime>("EditDate");

                    b.Property<string>("Title");

                    b.HasKey("Id");

                    b.HasIndex("BlogId");

                    b.ToTable("Post");
                });

                modelBuilder.Entity("ModelSnapshot22.Post", b =>
                {
                    b.HasOne("ModelSnapshot22.Blog", "Blog")
                        .WithMany("Posts")
                        .HasForeignKey("BlogId");
                });
#pragma warning restore 612, 618
            }
        }

        [ConditionalFact(Skip = "Implement")]
        public override void Can_diff_against_3_0_ASP_NET_Identity_model() {}

        [ConditionalFact(Skip = "Implement")]
        public override void Can_diff_against_2_2_ASP_NET_Identity_model() {}

        [ConditionalFact(Skip = "Implement")]
        public override void Can_diff_against_2_1_ASP_NET_Identity_model() {}

        public class MigrationsInfrastructureNpgsqlFixture : MigrationsInfrastructureFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;

            public override MigrationsContext CreateContext()
            {
                var options = AddOptions(
                        new DbContextOptionsBuilder()
                            .UseNpgsql(TestStore.ConnectionString, b => b.ApplyConfiguration().CommandTimeout(NpgsqlTestStore.CommandTimeout)))
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
