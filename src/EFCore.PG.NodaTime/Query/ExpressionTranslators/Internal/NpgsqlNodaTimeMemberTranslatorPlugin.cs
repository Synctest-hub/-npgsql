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
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using NodaTime;

// ReSharper disable once CheckNamespace
namespace Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime
{
    /// <summary>
    /// Provides translation services for <see cref="NodaTime"/> members.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/functions-datetime.html
    /// </remarks>
    public class NpgsqlNodaTimeMemberTranslatorPlugin : IMemberTranslatorPlugin
    {
        public NpgsqlNodaTimeMemberTranslatorPlugin(ISqlExpressionFactory sqlExpressionFactory)
        {
            Translators = new IMemberTranslator[]
            {
                new NpgsqlNodaTimeMemberTranslator(sqlExpressionFactory),
            };
        }

        public virtual IEnumerable<IMemberTranslator> Translators { get; }
    }

    /// <summary>
    /// Provides translation services for <see cref="NodaTime"/> members.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/functions-datetime.html
    /// </remarks>
    public class NpgsqlNodaTimeMemberTranslator : IMemberTranslator
    {
        readonly ISqlExpressionFactory _sqlExpressionFactory;

        /// <summary>
        /// The static member info for <see cref="T:SystemClock.Instance"/>.
        /// </summary>
        [NotNull] static readonly MemberInfo Instance =
            typeof(SystemClock).GetRuntimeProperty(nameof(SystemClock.Instance));

        public NpgsqlNodaTimeMemberTranslator(ISqlExpressionFactory sqlExpressionFactory)
            => _sqlExpressionFactory = sqlExpressionFactory;

        /// <inheritdoc />
        [CanBeNull]
        public SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType)
        {
            // This is necessary to allow translation of methods on SystemClock.Instance
            if (member == Instance)
                return _sqlExpressionFactory.Constant(SystemClock.Instance);

            var declaringType = member.DeclaringType;
            if (declaringType == typeof(LocalDateTime) ||
                declaringType == typeof(LocalDate) ||
                declaringType == typeof(LocalTime) ||
                declaringType == typeof(Period))
            {
                return TranslateDateTime(instance, member, returnType);
            }

            return null;
        }

        /// <summary>
        /// Translates date and time members.
        /// </summary>
        /// <param name="e">The member expression.</param>
        /// <returns>
        /// The translated expression or null.
        /// </returns>
        [CanBeNull]
        SqlExpression TranslateDateTime(SqlExpression instance, MemberInfo member, Type returnType)
        {
            switch (member.Name)
            {
            case "Year":
            case "Years":
                return GetDatePartExpression(instance, "year");

            case "Month":
            case "Months":
                return GetDatePartExpression(instance, "month");

            case "DayOfYear":
                return GetDatePartExpression(instance, "doy");

            case "Day":
            case "Days":
                return GetDatePartExpression(instance, "day");

            case "Hour":
            case "Hours":
                return GetDatePartExpression(instance, "hour");

            case "Minute":
            case "Minutes":
                return GetDatePartExpression(instance, "minute");

            case "Second":
            case "Seconds":
                return GetDatePartExpression(instance, "second", true);

            case "Millisecond":
            case "Milliseconds":
                return null; // Too annoying

            case "DayOfWeek":
                // Unlike DateTime.DayOfWeek, NodaTime's IsoDayOfWeek enum doesn't exactly correspond to PostgreSQL's
                // values returned by DATE_PART('dow', ...): in NodaTime Sunday is 7 and not 0, which is None.
                // So we generate a CASE WHEN expression to translate PostgreSQL's 0 to 7.
                var getValueExpression = GetDatePartExpression(instance, "dow", true);
                // TODO: Can be simplified once https://github.com/aspnet/EntityFrameworkCore/pull/16726 is in
                return
                    _sqlExpressionFactory.Case(
                        new[]
                        {
                            new CaseWhenClause(
                                _sqlExpressionFactory.Equal(getValueExpression, _sqlExpressionFactory.Constant(0)),
                                _sqlExpressionFactory.Constant(7))
                        },
                        getValueExpression
                    );

            case "Date":
                return _sqlExpressionFactory.Function("DATE_TRUNC", new[] { _sqlExpressionFactory.Constant("day"), instance }, returnType);

            case "TimeOfDay":
                // TODO: Technically possible simply via casting to PG time,
                // but ExplicitCastExpression only allows casting to PG types that
                // are default-mapped from CLR types (timespan maps to interval,
                // which timestamp cannot be cast into)
                return null;

            default:
                return null;
            }
        }

        /// <summary>
        /// Constructs the DATE_PART expression.
        /// </summary>
        /// <param name="e">The member expression.</param>
        /// <param name="partName">The name of the DATE_PART to construct.</param>
        /// <param name="floor">True if the result should be wrapped with FLOOR(...); otherwise, false.</param>
        /// <returns>
        /// The DATE_PART expression.
        /// </returns>
        /// <remarks>
        /// DATE_PART returns doubles, which we floor and cast into ints
        /// This also gets rid of sub-second components when retrieving seconds.
        /// </remarks>
        [NotNull]
        SqlExpression GetDatePartExpression(
            [NotNull] SqlExpression instance,
            [NotNull] string partName,
            bool floor = false)
        {
            var result = _sqlExpressionFactory.Function("DATE_PART", new[] { _sqlExpressionFactory.Constant(partName), instance }, typeof(double));

            if (floor)
                result = _sqlExpressionFactory.Function("FLOOR", new[] { result }, typeof(double));

            return _sqlExpressionFactory.Convert(result, typeof(int));
        }
    }
}