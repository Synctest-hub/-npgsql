﻿#region License

// The PostgreSQL License
//
// Copyright (C) 2016 The Npgsql Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.

#endregion

using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using Remotion.Linq.Parsing.ExpressionVisitors.TreeEvaluation;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.EvaluatableExpressionFilters.Internal
{
    // TODO: This is a hack until https://github.com/aspnet/EntityFrameworkCore/issues/13454 is done
    public class NpgsqlNodaTimeEvaluatableExpressionFilter : IEvaluatableExpressionFilter
    {
        /// <inheritdoc />
        public bool IsEvaluatableMethodCall(MethodCallExpression node)
            => node.Method.DeclaringType?.FullName != "NodaTime.SystemClock" ||
               node.Method.Name != "GetCurrentInstant";

        bool IEvaluatableExpressionFilter.IsEvaluatableMember(MemberExpression node)
            => node.Member.DeclaringType?.FullName != "NodaTime.SystemClock" ||
               node.Member.Name != "Instance";

        #region unused interface methods

        bool IEvaluatableExpressionFilter.IsEvaluatableBinary(BinaryExpression node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableConditional(ConditionalExpression node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableConstant(ConstantExpression node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableElementInit(ElementInit node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableInvocation(InvocationExpression node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableLambda(LambdaExpression node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableListInit(ListInitExpression node) => true;
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
