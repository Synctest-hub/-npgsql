﻿using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.ValueGeneration.Internal;

public class NpgsqlValueGeneratorSelector : RelationalValueGeneratorSelector
{
    private readonly INpgsqlSequenceValueGeneratorFactory _sequenceFactory;
    private readonly INpgsqlRelationalConnection _connection;
    private readonly IRawSqlCommandBuilder _rawSqlCommandBuilder;
    private readonly IRelationalCommandDiagnosticsLogger _commandLogger;

    public NpgsqlValueGeneratorSelector(
        ValueGeneratorSelectorDependencies dependencies,
        INpgsqlSequenceValueGeneratorFactory sequenceFactory,
        INpgsqlRelationalConnection connection,
        IRawSqlCommandBuilder rawSqlCommandBuilder,
        IRelationalCommandDiagnosticsLogger commandLogger)
        : base(dependencies)
    {
        _sequenceFactory = sequenceFactory;
        _connection = connection;
        _rawSqlCommandBuilder = rawSqlCommandBuilder;
        _commandLogger = commandLogger;
    }

    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public new virtual INpgsqlValueGeneratorCache Cache => (INpgsqlValueGeneratorCache)base.Cache;

    public override ValueGenerator Select(IProperty property, IEntityType entityType)
    {
        Check.NotNull(property, nameof(property));
        Check.NotNull(entityType, nameof(entityType));

        return property.GetValueGeneratorFactory() is null
            && property.GetValueGenerationStrategy() == NpgsqlValueGenerationStrategy.SequenceHiLo
                ? _sequenceFactory.Create(
                    property,
                    Cache.GetOrAddSequenceState(property, _connection),
                    _connection,
                    _rawSqlCommandBuilder,
                    _commandLogger)
                : base.Select(property, entityType);
    }

    public override ValueGenerator Create(IProperty property, IEntityType entityType)
    {
        Check.NotNull(property, nameof(property));
        Check.NotNull(entityType, nameof(entityType));

        // Generate temporary values if the user specified a default value (to allow
        // generating server-side with uuid-ossp or whatever)
        return property.ClrType.UnwrapNullableType() == typeof(Guid)
            ? property.ValueGenerated == ValueGenerated.Never
            || property.GetDefaultValueSql() is not null
                ? new TemporaryGuidValueGenerator()
                : new GuidValueGenerator()
            : base.Create(property, entityType);
    }
}