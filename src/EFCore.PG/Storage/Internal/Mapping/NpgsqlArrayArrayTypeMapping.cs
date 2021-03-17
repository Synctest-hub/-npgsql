﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Storage;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.ValueConversion;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    /// <summary>
    /// Maps PostgreSQL arrays to .NET arrays. Only single-dimensional arrays are supported.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Note that mapping PostgreSQL arrays to .NET <see cref="List{T}"/> is also supported via
    /// <see cref="NpgsqlArrayListTypeMapping"/>.
    /// </para>
    ///
    /// <para>See: https://www.postgresql.org/docs/current/static/arrays.html</para>
    /// </remarks>
    public class NpgsqlArrayArrayTypeMapping : NpgsqlArrayTypeMapping
    {
        /// <summary>
        /// Creates the default array mapping (i.e. for the single-dimensional CLR array type)
        /// </summary>
        /// <param name="storeType">The database type to map.</param>
        /// <param name="elementMapping">The element type mapping.</param>
        public NpgsqlArrayArrayTypeMapping(string storeType, RelationalTypeMapping elementMapping)
            : this(storeType, elementMapping, elementMapping.ClrType.MakeArrayType()) {}

        /// <summary>
        /// Creates the default array mapping (i.e. for the single-dimensional CLR array type)
        /// </summary>
        /// <param name="elementMapping">The element type mapping.</param>
        /// <param name="arrayType">The array type to map.</param>
        public NpgsqlArrayArrayTypeMapping(RelationalTypeMapping elementMapping, Type arrayType)
            : this(elementMapping.StoreType + "[]", elementMapping, arrayType) {}

        private NpgsqlArrayArrayTypeMapping(string storeType, RelationalTypeMapping elementMapping, Type arrayType)
            : this(
                new RelationalTypeMappingParameters(
                    new CoreTypeMappingParameters(
                        arrayType,
                        elementMapping.Converter is ValueConverter elementConverter
                            ? (ValueConverter)Activator.CreateInstance(
                                typeof(NpgsqlArrayConverter<,>).MakeGenericType(
                                    elementConverter.ModelClrType.MakeArrayType(),
                                    elementConverter.ProviderClrType.MakeArrayType()),
                                elementConverter)!
                            : null,
                        CreateComparer(elementMapping, arrayType)),
                    storeType
                ), elementMapping)
        {
        }

        protected NpgsqlArrayArrayTypeMapping(
            RelationalTypeMappingParameters parameters,
            RelationalTypeMapping elementMapping,
            bool? isElementNullable = null)
            : base(
                parameters,
                elementMapping,
                CalculateElementNullability(
                    // Note that the ClrType on elementMapping has been unwrapped for nullability, so we consult the array's CLR type instead
                    parameters.CoreParameters.Converter is null
                        ? (parameters.CoreParameters.ClrType.GetElementType()
                            ?? throw new ArgumentException("CLR type isn't an array"))
                        : (parameters.CoreParameters.Converter.ModelClrType.GetElementType()
                            ?? throw new ArgumentException("CLR type isn't an array")),
                    isElementNullable))
        {
            if (!parameters.CoreParameters.ClrType.IsArray)
                throw new ArgumentException("ClrType must be an array", nameof(parameters));
        }

        public override NpgsqlArrayTypeMapping MakeNonNullable()
            => new NpgsqlArrayArrayTypeMapping(Parameters, ElementMapping, isElementNullable: false);

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters, RelationalTypeMapping elementMapping)
            => new NpgsqlArrayArrayTypeMapping(parameters, elementMapping);

        #region Value comparer

        private static ValueComparer? CreateComparer(RelationalTypeMapping elementMapping, Type arrayType)
        {
            Debug.Assert(arrayType.IsArray);
            var elementType = arrayType.GetElementType()!;
            var unwrappedType = elementType.UnwrapNullableType();

            // We currently don't support mapping multi-dimensional arrays.
            if (arrayType.GetArrayRank() != 1)
                return null;

            return (ValueComparer)Activator.CreateInstance(
                elementType == unwrappedType
                    ? typeof(SingleDimensionalArrayComparer<>).MakeGenericType(elementType)
                    : typeof(NullableSingleDimensionalArrayComparer<>).MakeGenericType(unwrappedType),
                elementMapping)!;
        }

        private sealed class SingleDimensionalArrayComparer<TElem> : ValueComparer<TElem[]>
        {
            public SingleDimensionalArrayComparer(RelationalTypeMapping elementMapping) : base(
                (a, b) => Compare(a, b, (ValueComparer<TElem>)elementMapping.Comparer),
                o => GetHashCode(o, (ValueComparer<TElem>)elementMapping.Comparer),
                source => Snapshot(source, (ValueComparer<TElem>)elementMapping.Comparer)) {}

            public override Type Type => typeof(TElem[]);

            private static bool Compare(TElem[]? a, TElem[]? b, ValueComparer<TElem> elementComparer)
            {
                if (a is null)
                {
                    return b is null;
                }

                if (b is null || a.Length != b.Length)
                {
                    return false;
                }

                // Note: the following currently boxes every element access because ValueComparer isn't really
                // generic (see https://github.com/aspnet/EntityFrameworkCore/issues/11072)
                for (var i = 0; i < a.Length; i++)
                    if (!elementComparer.Equals(a[i], b[i]))
                        return false;

                return true;
            }

            private static int GetHashCode(TElem[] source, ValueComparer<TElem> elementComparer)
            {
                var hash = new HashCode();
                foreach (var el in source)
                    hash.Add(el, elementComparer);
                return hash.ToHashCode();
            }

            [return: NotNullIfNotNull("source")]
            private static TElem[]? Snapshot(TElem[]? source, ValueComparer<TElem> elementComparer)
            {
                if (source == null)
                    return null;

                var snapshot = new TElem[source.Length];
                // Note: the following currently boxes every element access because ValueComparer isn't really
                // generic (see https://github.com/aspnet/EntityFrameworkCore/issues/11072)
                for (var i = 0; i < source.Length; i++)
                    snapshot[i] = elementComparer.Snapshot(source[i])!; // TODO: https://github.com/dotnet/efcore/pull/24410
                return snapshot;
            }
        }

        private sealed class NullableSingleDimensionalArrayComparer<TElem> : ValueComparer<TElem?[]>
            where TElem : struct
        {
            public NullableSingleDimensionalArrayComparer(RelationalTypeMapping elementMapping) : base(
                (a, b) => Compare(a, b, (ValueComparer<TElem>)elementMapping.Comparer),
                o => GetHashCode(o, (ValueComparer<TElem>)elementMapping.Comparer),
                source => Snapshot(source, (ValueComparer<TElem>)elementMapping.Comparer)) {}

            public override Type Type => typeof(TElem?[]);

            private static bool Compare(TElem?[]? a, TElem?[]? b, ValueComparer<TElem> elementComparer)
            {
                if (a is null)
                {
                    return b is null;
                }

                if (b is null || a.Length != b.Length)
                {
                    return false;
                }

                // Note: the following currently boxes every element access because ValueComparer isn't really
                // generic (see https://github.com/aspnet/EntityFrameworkCore/issues/11072)
                for (var i = 0; i < a.Length; i++)
                {
                    var (el1, el2) = (a[i], b[i]);
                    if (el1 is null)
                    {
                        if (el2 is null)
                            continue;
                        return false;
                    }
                    if (el2 is null || !elementComparer.Equals(el1, el2))
                        return false;
                }

                return true;
            }

            private static int GetHashCode(TElem?[] source, ValueComparer<TElem> elementComparer)
            {
                var nullableEqualityComparer = new NullableEqualityComparer<TElem>(elementComparer);
                var hash = new HashCode();
                foreach (var el in source)
                    hash.Add(el, nullableEqualityComparer);
                return hash.ToHashCode();
            }

            [return: NotNullIfNotNull("source")]
            private static TElem?[]? Snapshot(TElem?[]? source, ValueComparer<TElem> elementComparer)
            {
                if (source == null)
                    return null;

                var snapshot = new TElem?[source.Length];
                // Note: the following currently boxes every element access because ValueComparer isn't really
                // generic (see https://github.com/aspnet/EntityFrameworkCore/issues/11072)
                for (var i = 0; i < source.Length; i++)
                    snapshot[i] = source[i] is { } value ? elementComparer.Snapshot(value) : (TElem?)null;
                return snapshot;
            }
        }

        #endregion Value comparer
    }
}
