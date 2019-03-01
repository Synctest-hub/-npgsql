using System;
using System.Net.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class LoggingNpgsqlTest : LoggingRelationalTestBase<NpgsqlDbContextOptionsBuilder, NpgsqlOptionsExtension>
    {
        [Fact]
        public void Logs_context_initialization_admin_database()
        {
            const string testValue = "foo";
            Assert.Equal(
                ExpectedMessage($"AdminDatabase={testValue} " + DefaultOptions),
                ActualMessage(CreateOptionsBuilder(b => ((NpgsqlDbContextOptionsBuilder)b).UseAdminDatabase(testValue))));
        }

        [Fact]
        public void Logs_context_initialization_postgres_version()
        {
            const string testValue = "10.7";
            Assert.Equal(
                ExpectedMessage($"PostgresVersion={testValue} " + DefaultOptions),
                ActualMessage(CreateOptionsBuilder(b => ((NpgsqlDbContextOptionsBuilder)b).SetPostgresVersion(Version.Parse(testValue)))));
        }

        [Fact]
        public void Logs_context_initialization_provide_client_certificates_callback()
        {
            ProvideClientCertificatesCallback testCallback = (certificates) => { };
            Assert.Equal(
                ExpectedMessage($"ProvideClientCertificatesCallback " + DefaultOptions),
                ActualMessage(CreateOptionsBuilder(b => ((NpgsqlDbContextOptionsBuilder)b).ProvideClientCertificatesCallback(testCallback))));
        }

        [Fact]
        public void Logs_context_initialization_remote_certificate_validation_callback()
        {
            RemoteCertificateValidationCallback testCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            Assert.Equal(
                ExpectedMessage($"RemoteCertificateValidationCallback " + DefaultOptions),
                ActualMessage(CreateOptionsBuilder(b => ((NpgsqlDbContextOptionsBuilder)b).RemoteCertificateValidationCallback(testCallback))));
        }

        [Fact]
        public void Logs_context_initialization_reverse_null_ordering()
        {
            RemoteCertificateValidationCallback testCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            Assert.Equal(
                ExpectedMessage($"ReverseNullOrdering " + DefaultOptions),
                ActualMessage(CreateOptionsBuilder(b => ((NpgsqlDbContextOptionsBuilder)b).ReverseNullOrdering())));
        }

        [Fact]
        public void Logs_context_initialization_user_range_definitions()
        {
            RemoteCertificateValidationCallback testCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            Assert.Equal(
                ExpectedMessage($"UserRangeDefinitions=[{typeof(int)}=>int4range] " + DefaultOptions),
                ActualMessage(CreateOptionsBuilder(b => ((NpgsqlDbContextOptionsBuilder)b).MapRange<int>("int4range"))));
        }

        protected override DbContextOptionsBuilder CreateOptionsBuilder(
            Action<RelationalDbContextOptionsBuilder<NpgsqlDbContextOptionsBuilder, NpgsqlOptionsExtension>> relationalAction)
            => new DbContextOptionsBuilder().UseNpgsql("Data Source=LoggingNpgsqlTest.db", relationalAction);

        protected override string ProviderName => "Npgsql.EntityFrameworkCore.PostgreSQL";
    }
}
