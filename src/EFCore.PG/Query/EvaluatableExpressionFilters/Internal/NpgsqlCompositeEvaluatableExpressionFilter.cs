﻿using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Remotion.Linq.Parsing.ExpressionVisitors.TreeEvaluation;

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
                new NpgsqlNodaTimeEvaluatableExpressionFilter(),
                new NpgsqlPGroongaEvaluatableExpressionFilter()
            };

        /// <inheritdoc />
        public NpgsqlCompositeEvaluatableExpressionFilter([NotNull] IModel model) : base(model) {}

        /// <inheritdoc />
        public override bool IsEvaluatableMember(MemberExpression expression)
            => _filters.All(x => x.IsEvaluatableMember(expression)) && base.IsEvaluatableMember(expression);

        /// <inheritdoc />
        public override bool IsEvaluatableMethodCall(MethodCallExpression expression)
            => _filters.All(x => x.IsEvaluatableMethodCall(expression)) && base.IsEvaluatableMethodCall(expression);

        /// <summary>
        /// Adds additional dispatches to the filters list.
        /// </summary>
        /// <param name="filters">The filters.</param>
        public virtual void AddFilters([NotNull] [ItemNotNull] IEnumerable<IEvaluatableExpressionFilter> filters)
            => _filters.InsertRange(0, filters);
    }
}
