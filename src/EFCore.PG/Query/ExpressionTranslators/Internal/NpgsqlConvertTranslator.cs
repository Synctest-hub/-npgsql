﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Translates methods defined on <see cref="T:System.Convert"/> into PostgreSQL CAST expressions.
    /// </summary>
    public class NpgsqlConvertTranslator : IMethodCallTranslator
    {
        static readonly Dictionary<string, string> TypeMapping = new Dictionary<string, string>
        {
            [nameof(Convert.ToBoolean)] = "bool",
            [nameof(Convert.ToByte)]    = "smallint",
            [nameof(Convert.ToDecimal)] = "numeric",
            [nameof(Convert.ToDouble)]  = "double precision",
            [nameof(Convert.ToInt16)]   = "smallint",
            [nameof(Convert.ToInt32)]   = "int",
            [nameof(Convert.ToInt64)]   = "bigint",
            [nameof(Convert.ToString)]  = "text"
        };

        static readonly List<Type> SupportedTypes = new List<Type>
        {
            typeof(bool),
            typeof(byte),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(int),
            typeof(long),
            typeof(short),
            typeof(string)
        };

        static readonly IEnumerable<MethodInfo> SupportedMethods =
            TypeMapping.Keys
                .SelectMany(t => typeof(Convert).GetTypeInfo().GetDeclaredMethods(t)
                    .Where(m => m.GetParameters().Length == 1
                                && SupportedTypes.Contains(m.GetParameters().First().ParameterType)));

        /// <inheritdoc />
        [CanBeNull]
        public virtual Expression Translate(MethodCallExpression methodCallExpression)
            => SupportedMethods.Contains(methodCallExpression.Method)
                ? new ExplicitStoreTypeCastExpression(
                    methodCallExpression.Arguments[0],
                    methodCallExpression.Type,
                    TypeMapping[methodCallExpression.Method.Name])
                : null;
    }
}
