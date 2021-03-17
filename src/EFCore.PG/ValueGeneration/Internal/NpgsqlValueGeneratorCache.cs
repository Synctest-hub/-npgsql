﻿using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.ValueGeneration.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class NpgsqlValueGeneratorCache : ValueGeneratorCache, INpgsqlValueGeneratorCache
    {
        private readonly ConcurrentDictionary<string, NpgsqlSequenceValueGeneratorState> _sequenceGeneratorCache
            = new();

        /// <summary>
        ///     Initializes a new instance of the <see cref="ValueGeneratorCache" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public NpgsqlValueGeneratorCache(ValueGeneratorCacheDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual NpgsqlSequenceValueGeneratorState GetOrAddSequenceState(
            IProperty property,
            IRelationalConnection connection)
        {
            var sequence = property.FindHiLoSequence();

            Debug.Assert(sequence != null);

            return _sequenceGeneratorCache.GetOrAdd(
                GetSequenceName(sequence, connection),
                sequenceName => new NpgsqlSequenceValueGeneratorState(sequence));
        }

        private static string GetSequenceName(ISequence sequence, IRelationalConnection connection)
        {
            var dbConnection = connection.DbConnection;

            return dbConnection.Database.ToUpperInvariant()
                   + "::"
                   + dbConnection.DataSource?.ToUpperInvariant()
                   + "::"
                   + (sequence.Schema == null ? "" : sequence.Schema + ".") + sequence.Name;
        }
    }
}
