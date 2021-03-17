using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

// ReSharper disable once CheckNamespace
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal
{
    public class NpgsqlNetTopologySuiteOptionsExtension : IDbContextOptionsExtension
    {
        private DbContextOptionsExtensionInfo? _info;

        public virtual bool IsGeographyDefault { get; private set; }

        public NpgsqlNetTopologySuiteOptionsExtension() {}

        protected NpgsqlNetTopologySuiteOptionsExtension(NpgsqlNetTopologySuiteOptionsExtension copyFrom)
            => IsGeographyDefault = copyFrom.IsGeographyDefault;

        protected virtual NpgsqlNetTopologySuiteOptionsExtension Clone() => new(this);

        public virtual void ApplyServices(IServiceCollection services)
            => services.AddEntityFrameworkNpgsqlNetTopologySuite();

        public virtual DbContextOptionsExtensionInfo Info
            => _info ??= new ExtensionInfo(this);

        public virtual NpgsqlNetTopologySuiteOptionsExtension WithGeographyDefault(bool isGeographyDefault = true)
        {
            var clone = Clone();

            clone.IsGeographyDefault = isGeographyDefault;

            return clone;
        }

        public virtual void Validate(IDbContextOptions options)
        {
            Check.NotNull(options, nameof(options));

            var internalServiceProvider = options.FindExtension<CoreOptionsExtension>()?.InternalServiceProvider;
            if (internalServiceProvider != null)
            {
                using (var scope = internalServiceProvider.CreateScope())
                {
                    if (scope.ServiceProvider.GetService<IEnumerable<IRelationalTypeMappingSourcePlugin>>()
                            ?.Any(s => s is NpgsqlNetTopologySuiteTypeMappingSourcePlugin) != true)
                    {
                        throw new InvalidOperationException($"{nameof(NpgsqlNetTopologySuiteDbContextOptionsBuilderExtensions.UseNetTopologySuite)} requires {nameof(NpgsqlNetTopologySuiteServiceCollectionExtensions.AddEntityFrameworkNpgsqlNetTopologySuite)} to be called on the internal service provider used.");
                    }
                }
            }
        }

        private sealed class ExtensionInfo : DbContextOptionsExtensionInfo
        {
            private string? _logFragment;

            public ExtensionInfo(IDbContextOptionsExtension extension)
                : base(extension)
            {
            }

            private new NpgsqlNetTopologySuiteOptionsExtension Extension
                => (NpgsqlNetTopologySuiteOptionsExtension)base.Extension;

            public override bool IsDatabaseProvider => false;

            public override long GetServiceProviderHashCode() => Extension.IsGeographyDefault.GetHashCode();

            public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
            {
                Check.NotNull(debugInfo, nameof(debugInfo));

                var prefix = "Npgsql:" + nameof(NpgsqlNetTopologySuiteDbContextOptionsBuilderExtensions.UseNetTopologySuite);
                debugInfo[prefix] = "1";
                debugInfo[$"{prefix}:{nameof(IsGeographyDefault)}"] = Extension.IsGeographyDefault.ToString();
            }

            public override string LogFragment
            {
                get
                {
                    if (_logFragment == null)
                    {
                        var builder = new StringBuilder("using NetTopologySuite");
                        if (Extension.IsGeographyDefault)
                            builder.Append(" (geography by default)");
                        builder.Append(' ');

                        _logFragment = builder.ToString();
                    }

                    return _logFragment;
                }
            }
        }
    }
}
