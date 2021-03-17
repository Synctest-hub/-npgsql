﻿using System;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.ValueConversion;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    /// <summary>
    /// Maps PostgreSQL arrays to <see cref="List{T}"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Note that mapping PostgreSQL arrays to .NET arrays is also supported via <see cref="NpgsqlArrayArrayTypeMapping"/>.
    /// </para>
    ///
    /// <para>See: https://www.postgresql.org/docs/current/static/arrays.html</para>
    /// </remarks>
    public class NpgsqlArrayListTypeMapping : NpgsqlArrayTypeMapping
    {
        /// <summary>
        /// Creates the default list mapping.
        /// </summary>
        /// <param name="storeType">The database type to map.</param>
        /// <param name="elementMapping">The element type mapping.</param>
        public NpgsqlArrayListTypeMapping(string storeType, RelationalTypeMapping elementMapping)
            : this(storeType, elementMapping, typeof(List<>).MakeGenericType(elementMapping.ClrType)) {}

        /// <summary>
        /// Creates the default list mapping.
        /// </summary>
        /// <param name="elementMapping">The element type mapping.</param>
        /// <param name="listType">The database type to map.</param>
        public NpgsqlArrayListTypeMapping(RelationalTypeMapping elementMapping, Type listType)
            : this(elementMapping.StoreType + "[]", elementMapping, listType) {}

        private NpgsqlArrayListTypeMapping(string storeType, RelationalTypeMapping elementMapping, Type listType)
            : this(
                new RelationalTypeMappingParameters(
                    new CoreTypeMappingParameters(
                        listType,
                        elementMapping.Converter is ValueConverter elementConverter
                            ? (ValueConverter)Activator.CreateInstance(
                                typeof(NpgsqlArrayConverter<,>).MakeGenericType(
                                    typeof(List<>).MakeGenericType(elementConverter.ModelClrType),
                                    elementConverter.ProviderClrType.MakeArrayType()),
                                elementConverter)!
                            : null,
                        CreateComparer(elementMapping, listType)),
                    storeType
                ), elementMapping)
        {
        }

        protected NpgsqlArrayListTypeMapping(
            RelationalTypeMappingParameters parameters, RelationalTypeMapping elementMapping, bool? isElementNullable = null)
            : base(
                parameters,
                elementMapping,
                CalculateElementNullability(
                    // Note that the ClrType on elementMapping has been unwrapped for nullability, so we consult the List's CLR type instead
                    parameters.CoreParameters.Converter is null
                        ? parameters.CoreParameters.ClrType.GetGenericArguments()[0]
                        : parameters.CoreParameters.Converter.ModelClrType.GetGenericArguments()[0],
                    isElementNullable))
        {
            if (!parameters.CoreParameters.ClrType.IsGenericList())
                throw new ArgumentException("ClrType must be a generic List", nameof(parameters));
        }

        public override NpgsqlArrayTypeMapping MakeNonNullable()
            => new NpgsqlArrayListTypeMapping(Parameters, ElementMapping, isElementNullable: false);

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters, RelationalTypeMapping elementMapping)
            => new NpgsqlArrayListTypeMapping(parameters, elementMapping);

        #region Value Comparison

        // Note that the value comparison code is largely duplicated from NpgsqlArrayTypeMapping.
        // However, a limitation in EF Core prevents us from merging the code together, see
        // https://github.com/aspnet/EntityFrameworkCore/issues/11077

        private static ValueComparer CreateComparer(RelationalTypeMapping elementMapping, Type listType)
        {
            Debug.Assert(listType.IsGenericType && listType.GetGenericTypeDefinition() == typeof(List<>));

            var elementType = listType.GetGenericArguments()[0];
            var unwrappedType = elementType.UnwrapNullableType();

            return (ValueComparer)Activator.CreateInstance(
                elementType == unwrappedType
                    ? typeof(ListComparer<>).MakeGenericType(elementType)
                    : typeof(NullableListComparer<>).MakeGenericType(unwrappedType),
                elementMapping)!;
        }

        private sealed class ListComparer<TElem> : ValueComparer<List<TElem>>
        {
            public ListComparer(RelationalTypeMapping elementMapping)
                : base(
                    (a, b) => Compare(a, b, (ValueComparer<TElem>)elementMapping.Comparer),
                    o => GetHashCode(o, (ValueComparer<TElem>)elementMapping.Comparer),
                    source => Snapshot(source, (ValueComparer<TElem>)elementMapping.Comparer)) {}

            public override Type Type => typeof(List<TElem>);

            private static bool Compare(List<TElem>? a, List<TElem>? b, ValueComparer<TElem> elementComparer)
            {
                if (a is null)
                {
                    return b is null;
                }

                if (b is null || a.Count != b.Count)
                {
                    return false;
                }

                // Note: the following currently boxes every element access because ValueComparer isn't really
                // generic (see https://github.com/aspnet/EntityFrameworkCore/issues/11072)
                for (var i = 0; i < a.Count; i++)
                    if (!elementComparer.Equals(a[i], b[i]))
                        return false;

                return true;
            }

            private static int GetHashCode(List<TElem> source, ValueComparer<TElem> elementComparer)
            {
                var hash = new HashCode();
                foreach (var el in source)
                    hash.Add(el, elementComparer);
                return hash.ToHashCode();
            }

            private static List<TElem>? Snapshot(List<TElem>? source, ValueComparer<TElem> elementComparer)
            {
                if (source == null)
                    return null;

                var snapshot = new List<TElem>(source.Count);

                // Note: the following currently boxes every element access because ValueComparer isn't really
                // generic (see https://github.com/aspnet/EntityFrameworkCore/issues/11072)
                foreach (var e in source)
                    snapshot.Add(elementComparer.Snapshot(e)!); // TODO: https://github.com/dotnet/efcore/pull/24410

                return snapshot;
            }
        }

        private sealed class NullableListComparer<TElem> : ValueComparer<List<TElem?>>
            where TElem : struct
        {
            public NullableListComparer(RelationalTypeMapping elementMapping)
                : base(
                    (a, b) => Compare(a, b, (ValueComparer<TElem>)elementMapping.Comparer),
                    o => GetHashCode(o, (ValueComparer<TElem>)elementMapping.Comparer),
                    source => Snapshot(source, (ValueComparer<TElem>)elementMapping.Comparer)) {}

            public override Type Type => typeof(List<TElem?>);

            private static bool Compare(List<TElem?>? a, List<TElem?>? b, ValueComparer<TElem> elementComparer)
            {
                if (a is null)
                {
                    return b is null;
                }

                if (b is null || a.Count != b.Count)
                {
                    return false;
                }

                // Note: the following currently boxes every element access because ValueComparer isn't really
                // generic (see https://github.com/aspnet/EntityFrameworkCore/issues/11072)
                for (var i = 0; i < a.Count; i++)
                {
                    var (el1, el2) = (a[i], b[i]);
                    if (el1 is null)
                    {
                        if (el2 is null)
                            continue;
                        return false;
                    }
                    if (el2 is null || !elementComparer.Equals(a[i], b[i]))
                        return false;
                }

                return true;
            }

            private static int GetHashCode(List<TElem?> source, ValueComparer<TElem> elementComparer)
            {
                var nullableEqualityComparer = new NullableEqualityComparer<TElem>(elementComparer);
                var hash = new HashCode();
                foreach (var el in source)
                    hash.Add(el, nullableEqualityComparer);
                return hash.ToHashCode();
            }

            private static List<TElem?>? Snapshot(List<TElem?>? source, ValueComparer<TElem> elementComparer)
            {
                if (source == null)
                    return null;

                var snapshot = new List<TElem?>(source.Count);

                // Note: the following currently boxes every element access because ValueComparer isn't really
                // generic (see https://github.com/aspnet/EntityFrameworkCore/issues/11072)
                foreach (var e in source)
                    snapshot.Add(e is { } value ? elementComparer.Snapshot(value) : (TElem?)null);

                return snapshot;
            }
        }

        #endregion Value Comparison
    }
}
