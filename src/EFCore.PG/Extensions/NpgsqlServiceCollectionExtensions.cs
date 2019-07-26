using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Npgsql.EntityFrameworkCore.PostgreSQL.Diagnostics.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Conventions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Update.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.ValueGeneration.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provides extension methods to configure Entity Framework Core for Npgsql.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public static class NpgsqlEntityFrameworkServicesBuilderExtensions
    {
        /// <summary>
        ///     <para>
        ///         Adds the services required by the Npgsql database provider for Entity Framework
        ///         to an <see cref="IServiceCollection" />. You use this method when using dependency injection
        ///         in your application, such as with ASP.NET. For more information on setting up dependency
        ///         injection, see http://go.microsoft.com/fwlink/?LinkId=526890.
        ///     </para>
        ///     <para>
        ///         You only need to use this functionality when you want Entity Framework to resolve the services it uses
        ///         from an external <see cref="IServiceCollection" />. If you are not using an external
        ///         <see cref="IServiceCollection" /> Entity Framework will take care of creating the services it requires.
        ///     </para>
        /// </summary>
        /// <example>
        ///     <code>
        ///         public void ConfigureServices(IServiceCollection services)
        ///         {
        ///             var connectionString = "connection string to database";
        ///
        ///             services
        ///                 .AddEntityFrameworkSqlServer()
        ///                 .AddDbContext&lt;MyContext&gt;(options => options.UseNpgsql(connectionString));
        ///         }
        ///     </code>
        /// </example>
        /// <param name="serviceCollection"> The <see cref="IServiceCollection" /> to add services to. </param>
        /// <returns>
        ///     A builder that allows further Entity Framework specific setup of the <see cref="IServiceCollection" />.
        /// </returns>
        public static IServiceCollection AddEntityFrameworkNpgsql([NotNull] this IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            var builder =
                new EntityFrameworkRelationalServicesBuilder(serviceCollection)
                    .TryAdd<LoggingDefinitions, NpgsqlLoggingDefinitions>()
                    .TryAdd<IDatabaseProvider, DatabaseProvider<NpgsqlOptionsExtension>>()
                    .TryAdd<IValueGeneratorCache>(p => p.GetService<INpgsqlValueGeneratorCache>())
                    .TryAdd<IRelationalTypeMappingSource, NpgsqlTypeMappingSource>()
                    .TryAdd<ISqlGenerationHelper, NpgsqlSqlGenerationHelper>()
                    .TryAdd<IMigrationsAnnotationProvider, NpgsqlMigrationsAnnotationProvider>()
                    .TryAdd<IModelValidator, NpgsqlModelValidator>()
                    .TryAdd<IProviderConventionSetBuilder, NpgsqlConventionSetBuilder>()
                    .TryAdd<IRelationalValueBufferFactoryFactory, TypedRelationalValueBufferFactoryFactory>()
                    .TryAdd<IUpdateSqlGenerator, NpgsqlUpdateSqlGenerator>()
                    .TryAdd<IModificationCommandBatchFactory, NpgsqlModificationCommandBatchFactory>()
                    .TryAdd<IValueGeneratorSelector, NpgsqlValueGeneratorSelector>()
                    .TryAdd<IRelationalConnection>(p => p.GetService<INpgsqlRelationalConnection>())
                    .TryAdd<IMigrationsSqlGenerator, NpgsqlMigrationsSqlGenerator>()
                    .TryAdd<IRelationalDatabaseCreator, NpgsqlDatabaseCreator>()
                    .TryAdd<IHistoryRepository, NpgsqlHistoryRepository>()
                    .TryAdd<ICompiledQueryCacheKeyGenerator, NpgsqlCompiledQueryCacheKeyGenerator>()
                    .TryAdd<IExecutionStrategyFactory, NpgsqlExecutionStrategyFactory>()
                    .TryAdd<IMethodCallTranslatorProvider, NpgsqlMethodCallTranslatorProvider>()
                    .TryAdd<IMemberTranslatorProvider, NpgsqlMemberTranslatorProvider>()
                    .TryAdd<IEvaluatableExpressionFilter, NpgsqlEvaluatableExpressionFilter>()
                    .TryAdd<IQuerySqlGeneratorFactory, NpgsqlQuerySqlGeneratorFactory>()
                    .TryAdd<IRelationalSqlTranslatingExpressionVisitorFactory, NpgsqlSqlTranslatingExpressionVisitorFactory>()
                    .TryAdd<ISqlExpressionFactory, NpgsqlSqlExpressionFactory>()
                    .TryAdd<ISingletonOptions, INpgsqlOptions>(p => p.GetService<INpgsqlOptions>())
                    .TryAddProviderSpecificServices(
                        b => b
                             .TryAddSingleton<INpgsqlValueGeneratorCache, NpgsqlValueGeneratorCache>()
                             .TryAddSingleton<INpgsqlOptions, NpgsqlOptions>()
                             .TryAddScoped<INpgsqlSequenceValueGeneratorFactory, NpgsqlSequenceValueGeneratorFactory>()
                             .TryAddScoped<INpgsqlRelationalConnection, NpgsqlRelationalConnection>());

            builder.TryAddCoreServices();

            return serviceCollection;
        }
    }
}
