﻿using System;
using System.Collections;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    /// <summary>
    /// Maps PostgreSQL arrays to <see cref="List{T}"/>.
    /// </summary>
    public class NpgsqlListTypeMapping : RelationalTypeMapping
    {
        /// <summary>
        /// The CLR type of the list items.
        /// </summary>
        public RelationalTypeMapping ElementMapping { get; }

        /// <summary>
        /// Creates the default list mapping.
        /// </summary>
        public NpgsqlListTypeMapping(RelationalTypeMapping elementMapping, Type listType)
            : this(elementMapping.StoreType + "[]", elementMapping, listType) {}

        /// <inheritdoc />
        NpgsqlListTypeMapping(string storeType, RelationalTypeMapping elementMapping, Type listType)
            : this(new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(listType, null, CreateComparer(elementMapping, listType)), storeType
            ), elementMapping) {}

        /// <inheritdoc />
        protected NpgsqlListTypeMapping(RelationalTypeMappingParameters parameters, RelationalTypeMapping elementMapping)
            : base(parameters)
            => ElementMapping = elementMapping;

        /// <inheritdoc />
        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlListTypeMapping(parameters, ElementMapping);

        /// <inheritdoc />
        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var list = (IList)value;

            if (list.GetType().GenericTypeArguments[0] != ElementMapping.ClrType)
                throw new NotSupportedException("Multidimensional array literals aren't supported");

            var sb = new StringBuilder();
            sb.Append("ARRAY[");
            for (var i = 0; i < list.Count; i++)
            {
                if (i > 0)
                    sb.Append(',');

                sb.Append(ElementMapping.GenerateSqlLiteral(list[i]));
            }

            sb.Append("]::");
            sb.Append(ElementMapping.StoreType);
            sb.Append("[]");
            return sb.ToString();
        }

        #region Value Comparison

        // Note that the value comparison code is largely duplicated from NpgsqlAraryTypeMapping.
        // However, a limitation in EF Core prevents us from merging the code together, see
        // https://github.com/aspnet/EntityFrameworkCore/issues/11077

        static ValueComparer CreateComparer(RelationalTypeMapping elementMapping, Type listType)
        {
            Debug.Assert(listType.IsGenericType && listType.GetGenericTypeDefinition() == typeof(List<>));

            var elementType = listType.GetGenericArguments()[0];

            // We use different comparer implementations based on whether we have a non-null element comparer,
            // and if not, whether the element is IEquatable<TElem>
            if (elementMapping.Comparer != null)
                return (ValueComparer)Activator.CreateInstance(typeof(SingleDimComparerWithComparer<>).MakeGenericType(elementType), elementMapping);

            if (typeof(IEquatable<>).MakeGenericType(elementType).IsAssignableFrom(elementType))
                return (ValueComparer)Activator.CreateInstance(typeof(SingleDimComparerWithIEquatable<>).MakeGenericType(elementType));

            // There's no custom comparer, and the element type doesn't implement IEquatable<TElem>. We have
            // no choice but to use the non-generic Equals method.
            return (ValueComparer)Activator.CreateInstance(typeof(SingleDimComparerWithEquals<>).MakeGenericType(elementType));
        }

        class SingleDimComparerWithComparer<TElem> : ValueComparer<List<TElem>>
        {
            public SingleDimComparerWithComparer(RelationalTypeMapping elementMapping)
                : base(
                    (a, b) => Compare(a, b, (ValueComparer<TElem>)elementMapping.Comparer),
                    o => o.GetHashCode(), // TODO: Need to get hash code of elements...
                    source => Snapshot(source, (ValueComparer<TElem>)elementMapping.Comparer)) {}

            public override Type Type => typeof(List<TElem>);

            static bool Compare(List<TElem> a, List<TElem> b, ValueComparer<TElem> elementComparer)
            {
                if (a.Count != b.Count)
                    return false;

                // Note: the following currently boxes every element access because ValueComparer isn't really
                // generic (see https://github.com/aspnet/EntityFrameworkCore/issues/11072)
                for (var i = 0; i < a.Count; i++)
                    if (!elementComparer.Equals(a[i], b[i]))
                        return false;

                return true;
            }

            static List<TElem> Snapshot(List<TElem> source, ValueComparer<TElem> elementComparer)
            {
                if (source == null)
                    return null;

                var snapshot = new List<TElem>(source.Count);

                // Note: the following currently boxes every element access because ValueComparer isn't really
                // generic (see https://github.com/aspnet/EntityFrameworkCore/issues/11072)
                foreach (var e in source)
                    snapshot.Add(elementComparer.Snapshot(e));

                return snapshot;
            }
        }

        class SingleDimComparerWithIEquatable<TElem> : ValueComparer<List<TElem>> where TElem : IEquatable<TElem>
        {
            public SingleDimComparerWithIEquatable()
                : base(
                    (a, b) => Compare(a, b),
                    o => o.GetHashCode(), // TODO: Need to get hash code of elements...
                    source => DoSnapshot(source)) {}

            public override Type Type => typeof(List<TElem>);

            static bool Compare(List<TElem> a, List<TElem> b)
            {
                if (a.Count != b.Count)
                    return false;

                for (var i = 0; i < a.Count; i++)
                {
                    var elem1 = a[i];
                    var elem2 = b[i];
                    // Note: the following null checks are elided if TElem is a value type
                    if (elem1 == null)
                    {
                        if (elem2 == null)
                            continue;

                        return false;
                    }

                    if (!elem1.Equals(elem2))
                        return false;
                }

                return true;
            }

            static List<TElem> DoSnapshot(List<TElem> source)
            {
                if (source == null)
                    return null;

                var snapshot = new List<TElem>(source.Count);

                foreach (var e in source)
                    snapshot.Add(e);

                return snapshot;
            }
        }

        class SingleDimComparerWithEquals<TElem> : ValueComparer<List<TElem>>
        {
            public SingleDimComparerWithEquals()
                : base(
                    (a, b) => Compare(a, b),
                    o => o.GetHashCode(), // TODO: Need to get hash code of elements...
                    source => DoSnapshot(source)) {}

            public override Type Type => typeof(List<TElem>);

            static bool Compare(List<TElem> a, List<TElem> b)
            {
                if (a.Count != b.Count)
                    return false;

                // Note: the following currently boxes every element access because ValueComparer isn't really
                // generic (see https://github.com/aspnet/EntityFrameworkCore/issues/11072)
                for (var i = 0; i < a.Count; i++)
                {
                    var elem1 = a[i];
                    var elem2 = b[i];
                    if (elem1 == null)
                    {
                        if (elem2 == null)
                            continue;
                        return false;
                    }

                    if (!elem1.Equals(elem2))
                        return false;
                }

                return true;
            }

            static List<TElem> DoSnapshot(List<TElem> source)
            {
                if (source == null)
                    return null;

                var snapshot = new List<TElem>(source.Count);

                // Note: the following currently boxes every element access because ValueComparer isn't really
                // generic (see https://github.com/aspnet/EntityFrameworkCore/issues/11072)
                foreach (var e in source)
                    snapshot.Add(e);

                return snapshot;
            }
        }

        #endregion Value Comparison
    }
}
