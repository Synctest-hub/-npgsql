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
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Provides translation services for PostgreSQL network address (inet, cidr) operators.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/functions-net.html
    /// </remarks>
    public class NpgsqlNetworkAddressTranslator : IMethodCallTranslator
    {
        /// <inheritdoc />
        [CanBeNull]
        public Expression Translate(MethodCallExpression expression)
        {
            if (expression.Method.DeclaringType != typeof(NpgsqlNetworkAddressExtensions))
                return null;

            switch (expression.Method.Name)
            {
            case nameof(NpgsqlNetworkAddressExtensions.Contains):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], ">>", typeof(bool));

            case nameof(NpgsqlNetworkAddressExtensions.ContainsOrEqual):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], ">>=", typeof(bool));

            case nameof(NpgsqlNetworkAddressExtensions.LessThan):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], "<", typeof(bool));

            case nameof(NpgsqlNetworkAddressExtensions.LessThanOrEqual):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], "<=", typeof(bool));

            case nameof(NpgsqlNetworkAddressExtensions.Equal):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], "=", typeof(bool));

            case nameof(NpgsqlNetworkAddressExtensions.GreaterThanOrEqual):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], ">=", typeof(bool));

            case nameof(NpgsqlNetworkAddressExtensions.GreaterThan):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], ">", typeof(bool));

            case nameof(NpgsqlNetworkAddressExtensions.NotEqual):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], "<>", typeof(bool));

            case nameof(NpgsqlNetworkAddressExtensions.ContainedBy):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], "<<", typeof(bool));

            case nameof(NpgsqlNetworkAddressExtensions.ContainedByOrEqual):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], "<<=", typeof(bool));

            case nameof(NpgsqlNetworkAddressExtensions.Not):
                return new CustomUnaryExpression(expression.Arguments[1], "~", expression.Arguments[1].Type);

            case nameof(NpgsqlNetworkAddressExtensions.And):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], "&", expression.Arguments[1].Type);

            case nameof(NpgsqlNetworkAddressExtensions.Or):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], "|", expression.Arguments[1].Type);

            case nameof(NpgsqlNetworkAddressExtensions.Add):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], "+", expression.Arguments[1].Type);

            case nameof(NpgsqlNetworkAddressExtensions.Subtract):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], "-", expression.Arguments[1].Type);

            default:
                return null;
            }
        }
    }
}
