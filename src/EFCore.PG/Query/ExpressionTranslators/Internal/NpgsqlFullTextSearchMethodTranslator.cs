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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Provides translations for PostgreSQL full-text search methods.
    /// </summary>
    public class NpgsqlFullTextSearchMethodTranslator : IMethodCallTranslator
    {
        static readonly MethodInfo TsQueryParse =
            typeof(NpgsqlTsQuery).GetMethod(nameof(NpgsqlTsQuery.Parse), BindingFlags.Public | BindingFlags.Static);

        static readonly MethodInfo TsVectorParse =
            typeof(NpgsqlTsVector).GetMethod(nameof(NpgsqlTsVector.Parse), BindingFlags.Public | BindingFlags.Static);

        static readonly IReadOnlyDictionary<string, string> SqlNameByMethodName = new Dictionary<string, string>
        {
            [nameof(NpgsqlFullTextSearchDbFunctionsExtensions.ArrayToTsVector)] = "array_to_tsvector",
            [nameof(NpgsqlFullTextSearchDbFunctionsExtensions.ToTsVector)] = "to_tsvector",
            [nameof(NpgsqlFullTextSearchDbFunctionsExtensions.PlainToTsQuery)] = "plainto_tsquery",
            [nameof(NpgsqlFullTextSearchDbFunctionsExtensions.PhraseToTsQuery)] = "phraseto_tsquery",
            [nameof(NpgsqlFullTextSearchDbFunctionsExtensions.ToTsQuery)] = "to_tsquery"
        };

        /// <inheritdoc />
        [CanBeNull]
        public Expression Translate(MethodCallExpression e)
        {
            if (e.Method == TsQueryParse || e.Method == TsVectorParse)
                return new ExplicitCastExpression(e.Arguments[0], e.Method.ReturnType);

            if (e.Method.DeclaringType == typeof(NpgsqlFullTextSearchDbFunctionsExtensions)
                && SqlNameByMethodName.TryGetValue(e.Method.Name, out var sqlFunctionName))
                return new SqlFunctionExpression(sqlFunctionName, e.Method.ReturnType, e.Arguments.Skip(1));

            if (e.Method.DeclaringType == typeof(NpgsqlFullTextSearchLinqExtensions))
                return TryTranslateOperator(e) ?? TryTranslateFunction(e);

            return null;
        }

        [CanBeNull]
        static Expression TryTranslateOperator([NotNull] MethodCallExpression e)
        {
            switch (e.Method.Name)
            {
            case nameof(NpgsqlFullTextSearchLinqExtensions.And):
                return new CustomBinaryExpression(e.Arguments[0], e.Arguments[1], "&&", typeof(NpgsqlTsQuery));

            case nameof(NpgsqlFullTextSearchLinqExtensions.Or):
                return new CustomBinaryExpression(e.Arguments[0], e.Arguments[1], "||", typeof(NpgsqlTsQuery));

            case nameof(NpgsqlFullTextSearchLinqExtensions.ToNegative):
                return new CustomUnaryExpression(e.Arguments[0], "!!", typeof(NpgsqlTsQuery));

            case nameof(NpgsqlFullTextSearchLinqExtensions.Contains):
                return new CustomBinaryExpression(e.Arguments[0], e.Arguments[1], "@>", typeof(bool));

            case nameof(NpgsqlFullTextSearchLinqExtensions.IsContainedIn):
                return new CustomBinaryExpression(e.Arguments[0], e.Arguments[1], "<@", typeof(bool));

            case nameof(NpgsqlFullTextSearchLinqExtensions.Matches):
                var secondArgument = e.Arguments[1].Type == typeof(string)
                    ? new SqlFunctionExpression("plainto_tsquery", typeof(NpgsqlTsQuery), new[] { e.Arguments[1] })
                    : e.Arguments[1];
                return new CustomBinaryExpression(e.Arguments[0], secondArgument, "@@", typeof(bool));

            case nameof(NpgsqlFullTextSearchLinqExtensions.Concat):
                return new CustomBinaryExpression(e.Arguments[0], e.Arguments[1], "||", typeof(NpgsqlTsVector));

            default:
                return null;
            }
        }

        [CanBeNull]
        static Expression TryTranslateFunction([NotNull] MethodCallExpression e)
        {
            switch (e.Method.Name)
            {
            case nameof(NpgsqlFullTextSearchLinqExtensions.GetNodeCount):
                return new SqlFunctionExpression("numnode", e.Method.ReturnType, e.Arguments);

            case nameof(NpgsqlFullTextSearchLinqExtensions.GetQueryTree):
                return new SqlFunctionExpression("querytree", e.Method.ReturnType, e.Arguments);

            case nameof(NpgsqlFullTextSearchLinqExtensions.GetResultHeadline):
                switch (e.Arguments.Count)
                {
                case 2:
                    return new SqlFunctionExpression("ts_headline", e.Method.ReturnType, e.Arguments.Reverse());

                case 3:
                    return new SqlFunctionExpression(
                        "ts_headline",
                        e.Method.ReturnType,
                        new[]
                        {
                            e.Arguments[1],
                            e.Arguments[0],
                            e.Arguments[2]
                        });

                case 4:
                    return new SqlFunctionExpression(
                        "ts_headline",
                        e.Method.ReturnType,
                        new[]
                        {
                            e.Arguments[1],
                            e.Arguments[2],
                            e.Arguments[0],
                            e.Arguments[3]
                        });

                default:
                    throw new ArgumentException("Invalid method overload for ts_headline", nameof(e));
                }

            case nameof(NpgsqlFullTextSearchLinqExtensions.Rewrite):
                return new SqlFunctionExpression("ts_rewrite", e.Method.ReturnType, e.Arguments);

            case nameof(NpgsqlFullTextSearchLinqExtensions.ToPhrase):
                return new SqlFunctionExpression("tsquery_phrase", e.Method.ReturnType, e.Arguments);

            case nameof(NpgsqlFullTextSearchLinqExtensions.SetWeight):
                var arguments = e.Arguments.ToArray();

                if (arguments[1].Type == typeof(NpgsqlTsVector.Lexeme.Weight))
                {
                    if (!(arguments[1] is ConstantExpression weightExpression))
                    {
                        throw new ArgumentException(
                            "Enum 'weight' argument for 'SetWeight' must be a constant expression.");
                    }

                    arguments[1] = Expression.Constant(weightExpression.Value.ToString()[0]);
                }

                return new SqlFunctionExpression("setweight", e.Method.ReturnType, arguments);

            case nameof(NpgsqlFullTextSearchLinqExtensions.Delete):
                return new SqlFunctionExpression("ts_delete", e.Method.ReturnType, e.Arguments);

            case nameof(NpgsqlFullTextSearchLinqExtensions.Filter):
                return new SqlFunctionExpression(
                    "ts_filter",
                    e.Method.ReturnType,
                    new[]
                    {
                        e.Arguments[0],
                        new ExplicitStoreTypeCastExpression(e.Arguments[1], typeof(char[]), "\"char\"[]")
                    });

            case nameof(NpgsqlFullTextSearchLinqExtensions.GetLength):
                return new SqlFunctionExpression("length", e.Method.ReturnType, e.Arguments);

            case nameof(NpgsqlFullTextSearchLinqExtensions.ToStripped):
                return new SqlFunctionExpression("strip", e.Method.ReturnType, e.Arguments);

            case nameof(NpgsqlFullTextSearchLinqExtensions.Rank):
            case nameof(NpgsqlFullTextSearchLinqExtensions.RankCoverDensity):
                var rankFunctionName = e.Method.Name == nameof(NpgsqlFullTextSearchLinqExtensions.Rank)
                    ? "ts_rank"
                    : "ts_rank_cd";

                switch (e.Arguments.Count)
                {
                case 2:
                    return new SqlFunctionExpression(rankFunctionName, e.Method.ReturnType, e.Arguments);

                case 3:
                    var firstArgument = e.Arguments[0];
                    var secondArgument = e.Arguments[1];
                    if (e.Arguments[1].Type == typeof(float[]))
                    {
                        var temp = firstArgument;
                        firstArgument = secondArgument;
                        secondArgument = temp;
                    }

                    return new SqlFunctionExpression(
                        rankFunctionName,
                        e.Method.ReturnType,
                        new[]
                        {
                            firstArgument,
                            secondArgument,
                            e.Arguments[2]
                        });

                case 4:
                    return new SqlFunctionExpression(
                        rankFunctionName,
                        e.Method.ReturnType,
                        new[]
                        {
                            e.Arguments[1],
                            e.Arguments[0],
                            e.Arguments[2],
                            e.Arguments[3]
                        });

                default:
                    throw new ArgumentException(
                        $"Invalid method overload for {rankFunctionName}",
                        nameof(e));
                }

            default:
                return null;
            }
        }
    }
}
