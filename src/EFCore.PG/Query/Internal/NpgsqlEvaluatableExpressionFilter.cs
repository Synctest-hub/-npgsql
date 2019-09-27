using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal
{
    public class NpgsqlEvaluatableExpressionFilter : RelationalEvaluatableExpressionFilter
    {
        [NotNull] static readonly MethodInfo TsQueryParse =
            typeof(NpgsqlTsQuery).GetRuntimeMethod(nameof(NpgsqlTsQuery.Parse), new[] { typeof(string) });

        [NotNull] static readonly MethodInfo TsVectorParse =
            typeof(NpgsqlTsVector).GetRuntimeMethod(nameof(NpgsqlTsVector.Parse), new[] { typeof(string) });

        public NpgsqlEvaluatableExpressionFilter(
            [NotNull] EvaluatableExpressionFilterDependencies dependencies,
            [NotNull] RelationalEvaluatableExpressionFilterDependencies relationalDependencies)
            : base(dependencies, relationalDependencies)
        {}

        public override bool IsEvaluatableExpression(Expression expression, IModel model)
        {
            // Full text search
            if (expression is MethodCallExpression e && (
                e.Method == TsQueryParse || e.Method == TsVectorParse ||
                e.Method.DeclaringType == typeof(NpgsqlFullTextSearchDbFunctionsExtensions) ||
                e.Method.DeclaringType == typeof(NpgsqlFullTextSearchLinqExtensions)))
            {
                return false;
            }

            // NodaTime
            // TODO: This is a hack until https://github.com/aspnet/EntityFrameworkCore/issues/13454 is done
            if (expression is MethodCallExpression methodExpr && (
                    methodExpr.Method.DeclaringType?.FullName == "NodaTime.SystemClock" ||
                    methodExpr.Method.Name == "GetCurrentInstant") ||
                expression is MemberExpression memberExpr && (
                    memberExpr.Member.DeclaringType?.FullName == "NodaTime.SystemClock" ||
                    memberExpr.Member.Name == "Instance"))
            {
                return false;
            }

            // PGroonga
            if (expression is MethodCallExpression exp &&
                    exp.Method.DeclaringType?.FullName == "Microsoft.EntityFrameworkCore.PGroongaDbFunctionsExtensions")
            {
                return false;
            }

            return base.IsEvaluatableExpression(expression, model);
        }
    }
}
