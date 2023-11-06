using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

/// <summary>
/// The type mapping for the PostgreSQL inet type.
/// </summary>
/// <remarks>
/// See: https://www.postgresql.org/docs/current/static/datatype-net-types.html#DATATYPE-INET
/// </remarks>
public class NpgsqlInetTypeMapping : NpgsqlTypeMapping
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlInetTypeMapping(Type clrType)
        : base(
            "inet",
            clrType,
            NpgsqlDbType.Inet,
            jsonValueReaderWriter: clrType == typeof(IPAddress)
                ? JsonIPAddressReaderWriter.Instance
                : clrType == typeof(NpgsqlInet)
                    ? JsonNpgsqlInetReaderWriter.Instance
                    : throw new ArgumentException($"Only {nameof(IPAddress)} and {nameof(NpgsqlInet)} are supported", nameof(clrType)))
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected NpgsqlInetTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, NpgsqlDbType.Inet) {}

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new NpgsqlInetTypeMapping(parameters);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string GenerateNonNullSqlLiteral(object value)
        => $"INET '{value}'";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression GenerateCodeLiteral(object value)
        => value switch
        {
            IPAddress ip => Expression.Call(IPAddressParseMethod, Expression.Constant(ip.ToString())),
            NpgsqlInet ip => Expression.New(NpgsqlInetConstructor, Expression.Constant(ip.ToString())),
            _ => throw new UnreachableException()
        };

    private static readonly MethodInfo IPAddressParseMethod = typeof(IPAddress).GetMethod("Parse", new[] { typeof(string) })!;
    private static readonly ConstructorInfo NpgsqlInetConstructor = typeof(NpgsqlInet).GetConstructor(new[] { typeof(string) })!;

    private sealed class JsonIPAddressReaderWriter : JsonValueReaderWriter<IPAddress>
    {
        public static JsonIPAddressReaderWriter Instance { get; } = new();

        public override IPAddress FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
            => IPAddress.Parse(manager.CurrentReader.GetString()!);

        public override void ToJsonTyped(Utf8JsonWriter writer, IPAddress value)
            => writer.WriteStringValue(value.ToString());
    }

    private sealed class JsonNpgsqlInetReaderWriter : JsonValueReaderWriter<NpgsqlInet>
    {
        public static JsonNpgsqlInetReaderWriter Instance { get; } = new();

        public override NpgsqlInet FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
            => new(manager.CurrentReader.GetString()!);

        public override void ToJsonTyped(Utf8JsonWriter writer, NpgsqlInet value)
            => writer.WriteStringValue(value.ToString());
    }
}
