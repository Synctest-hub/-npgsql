﻿using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;

// Note: RelationalEvaluatableExpressionFilter will be disappearing in 3.0, at least in its current form
#pragma warning disable EF1001

// ReSharper disable UnusedMember.Global
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.EvaluatableExpressionFilters.Internal
{
    /// <summary>
    /// A composite evaluatable expression filter that dispatches to multiple specialized filters specific to Npgsql.
    /// </summary>
    public class NpgsqlCompositeEvaluatableExpressionFilter : RelationalEvaluatableExpressionFilter
    {
        /// <summary>
        /// The collection of registered evaluatable expression filters.
        /// </summary>
        [NotNull] [ItemNotNull] readonly List<IEvaluatableExpressionFilter> _filters =
            new List<IEvaluatableExpressionFilter>
            {
                new NpgsqlFullTextSearchEvaluatableExpressionFilter(),
                new NpgsqlNodaTimeEvaluatableExpressionFilter()
            };

        /// <inheritdoc />
        public NpgsqlCompositeEvaluatableExpressionFilter([NotNull] IModel model) : base(model) {}

        /// <inheritdoc />
        public override bool IsEvaluatableExpression(Expression expression)
            => _filters.All(x => x.IsEvaluatableExpression(expression)) && base.IsEvaluatableExpression(expression);

        /// <summary>
        /// Adds additional dispatches to the filters list.
        /// </summary>
        /// <param name="filters">The filters.</param>
        public virtual void AddFilters([NotNull] [ItemNotNull] IEnumerable<IEvaluatableExpressionFilter> filters)
            => _filters.InsertRange(0, filters);
    }
}
