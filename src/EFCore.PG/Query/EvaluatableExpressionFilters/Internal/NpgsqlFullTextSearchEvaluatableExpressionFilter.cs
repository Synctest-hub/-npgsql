﻿using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using Remotion.Linq.Parsing.ExpressionVisitors.TreeEvaluation;

// TODO: https://github.com/aspnet/EntityFrameworkCore/issues/11466 may provide a better way for implementing
// this (we're currently replacing an internal service
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.EvaluatableExpressionFilters.Internal
{
    /// <summary>
    /// Represents an Npgsql-specific filter for full-text search to identify expressions that are evaluatable.
    /// </summary>
    public class NpgsqlFullTextSearchEvaluatableExpressionFilter : IEvaluatableExpressionFilter
    {
        /// <summary>
        /// The static method info for <see cref="NpgsqlTsQuery.Parse(string)"/>.
        /// </summary>
        [NotNull] static readonly MethodInfo TsQueryParse =
            typeof(NpgsqlTsQuery).GetRuntimeMethod(nameof(NpgsqlTsQuery.Parse), new[] { typeof(string) });

        /// <summary>
        /// The static method info for <see cref="NpgsqlTsVector.Parse(string)"/>.
        /// </summary>
        [NotNull] static readonly MethodInfo TsVectorParse =
            typeof(NpgsqlTsVector).GetRuntimeMethod(nameof(NpgsqlTsVector.Parse), new[] { typeof(string) });

        /// <inheritdoc />
        public bool IsEvaluatableMethodCall(MethodCallExpression node)
            => node.Method != TsQueryParse &&
               node.Method != TsVectorParse &&
               node.Method.DeclaringType != typeof(NpgsqlFullTextSearchDbFunctionsExtensions) &&
               node.Method.DeclaringType != typeof(NpgsqlFullTextSearchLinqExtensions);

        #region unused interface methods

        bool IEvaluatableExpressionFilter.IsEvaluatableBinary(BinaryExpression node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableConditional(ConditionalExpression node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableConstant(ConstantExpression node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableElementInit(ElementInit node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableInvocation(InvocationExpression node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableLambda(LambdaExpression node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableListInit(ListInitExpression node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableMember(MemberExpression node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableMemberAssignment(MemberAssignment node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableMemberInit(MemberInitExpression node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableMemberListBinding(MemberListBinding node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableMemberMemberBinding(MemberMemberBinding node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableNew(NewExpression node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableNewArray(NewArrayExpression node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableTypeBinary(TypeBinaryExpression node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableUnary(UnaryExpression node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableBlock(BlockExpression node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableCatchBlock(CatchBlock node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableDebugInfo(DebugInfoExpression node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableDefault(DefaultExpression node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableGoto(GotoExpression node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableIndex(IndexExpression node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableLabel(LabelExpression node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableLabelTarget(LabelTarget node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableLoop(LoopExpression node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableSwitch(SwitchExpression node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableSwitchCase(SwitchCase node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableTry(TryExpression node) => true;

        #endregion
    }
}
