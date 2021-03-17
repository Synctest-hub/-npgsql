using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Security;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal
{
    public class NpgsqlRelationalConnection : RelationalConnection, INpgsqlRelationalConnection
    {
        private ProvideClientCertificatesCallback? ProvideClientCertificatesCallback { get; }
        private RemoteCertificateValidationCallback? RemoteCertificateValidationCallback { get; }
        private ProvidePasswordCallback? ProvidePasswordCallback { get; }

        /// <summary>
        ///     Indicates whether the store connection supports ambient transactions
        /// </summary>
        protected override bool SupportsAmbientTransactions => true;

        public NpgsqlRelationalConnection(RelationalConnectionDependencies dependencies)
            : base(dependencies)
        {
            var npgsqlOptions =
                dependencies.ContextOptions.Extensions.OfType<NpgsqlOptionsExtension>().FirstOrDefault();

            if (npgsqlOptions is not null)
            {
                ProvideClientCertificatesCallback = npgsqlOptions.ProvideClientCertificatesCallback;
                RemoteCertificateValidationCallback = npgsqlOptions.RemoteCertificateValidationCallback;
                ProvidePasswordCallback = npgsqlOptions.ProvidePasswordCallback;
            }
        }

        protected override DbConnection CreateDbConnection()
        {
            var conn = new NpgsqlConnection(ConnectionString);
            if (ProvideClientCertificatesCallback != null)
                conn.ProvideClientCertificatesCallback = ProvideClientCertificatesCallback;
            if (RemoteCertificateValidationCallback != null)
                conn.UserCertificateValidationCallback = RemoteCertificateValidationCallback;
            if (ProvidePasswordCallback != null)
                conn.ProvidePasswordCallback = ProvidePasswordCallback;
            return conn;
        }

        public virtual INpgsqlRelationalConnection CreateMasterConnection()
        {
            var adminDb = Dependencies.ContextOptions.FindExtension<NpgsqlOptionsExtension>()?.AdminDatabase
                          ?? "postgres";
            var csb = new NpgsqlConnectionStringBuilder(ConnectionString) {
                Database = adminDb,
                Pooling = false
            };

            var relationalOptions = RelationalOptionsExtension.Extract(Dependencies.ContextOptions);
            var connectionString = csb.ToString();

            relationalOptions = relationalOptions.Connection != null
                ? relationalOptions.WithConnection(((NpgsqlConnection)DbConnection).CloneWith(connectionString))
                : relationalOptions.WithConnectionString(connectionString);

            var optionsBuilder = new DbContextOptionsBuilder();
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(relationalOptions);

            return new NpgsqlRelationalConnection(Dependencies with { ContextOptions = optionsBuilder.Options });
        }

        [AllowNull]
        public new virtual NpgsqlConnection DbConnection
        {
            get => (NpgsqlConnection)base.DbConnection;
            set => base.DbConnection = value;
        }

        // Accessing Transaction.Current is expensive, so don't do it if Enlist is false in the connection string
        public override Transaction? CurrentAmbientTransaction
            => DbConnection.Settings.Enlist ? Transaction.Current : null;

        public virtual NpgsqlRelationalConnection CloneWith(string connectionString)
        {
            var clonedDbConnection = DbConnection.CloneWith(connectionString);

            var relationalOptions = RelationalOptionsExtension.Extract(Dependencies.ContextOptions)
                .WithConnection(clonedDbConnection);

            var optionsBuilder = new DbContextOptionsBuilder();
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(relationalOptions);

            return new NpgsqlRelationalConnection(Dependencies with { ContextOptions = optionsBuilder.Options });
        }
    }
}
