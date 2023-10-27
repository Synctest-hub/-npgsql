namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

/// <summary>
///     An expression that represents a PostgreSQL <c>unnest</c> function call in a SQL tree.
/// </summary>
/// <remarks>
///     <para>
///         This expression is just a <see cref="TableValuedFunctionExpression" />, adding the ability to provide an explicit column name
///         for its output (<c>SELECT * FROM unnest(array) AS f(foo)</c>). This is necessary since when the column name isn't explicitly
///         specified, it is automatically identical to the table alias (<c>f</c> above); since the table alias may get uniquified by
///         EF, this would break queries.
///     </para>
///     <para>
///         See <see href="https://www.postgresql.org/docs/current/functions-array.html#ARRAY-FUNCTIONS-TABLE">unnest</see> for more
///         information and examples.
///     </para>
///     <para>
///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///         the same compatibility standards as public APIs. It may be changed or removed without notice in
///         any release. You should only use it directly in your code with extreme caution and knowing that
///         doing so can result in application failures when updating to a new Entity Framework Core release.
///     </para>
/// </remarks>
public class PgUnnestExpression : TableValuedFunctionExpression
{
    /// <summary>
    ///     The array to be un-nested into a table.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public virtual SqlExpression Array
        => Arguments[0];

    /// <summary>
    ///     The name of the column to be projected out from the <c>unnest</c> call.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public virtual string ColumnName { get; }

    /// <summary>
    ///     Whether to project an additional ordinality column containing the index of each element in the array.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public virtual bool WithOrdinality { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public PgUnnestExpression(string alias, SqlExpression array, string columnName, bool withOrdinality = true)
        : base(alias, "unnest", schema: null, builtIn: true, new[] { array })
    {
        ColumnName = columnName;
        WithOrdinality = withOrdinality;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override TableValuedFunctionExpression Update(IReadOnlyList<SqlExpression> arguments)
        => arguments is [var singleArgument]
            ? Update(singleArgument)
            : throw new ArgumentException();

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="array">The <see cref="Array" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public virtual PgUnnestExpression Update(SqlExpression array)
        => array == Array
            ? this
            : new PgUnnestExpression(Alias, array, ColumnName, WithOrdinality);

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append(Name);
        expressionPrinter.Append("(");
        expressionPrinter.VisitCollection(Arguments);
        expressionPrinter.Append(")");

        if (WithOrdinality)
        {
            expressionPrinter.Append(" WITH ORDINALITY");
        }

        PrintAnnotations(expressionPrinter);

        expressionPrinter
            .Append(" AS ")
            .Append(Alias)
            .Append("(")
            .Append(ColumnName)
            .Append(")");
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is PgUnnestExpression unnestExpression
                && Equals(unnestExpression));

    private bool Equals(PgUnnestExpression unnestExpression)
        => base.Equals(unnestExpression)
            && ColumnName == unnestExpression.ColumnName
            && WithOrdinality == unnestExpression.WithOrdinality;

    /// <inheritdoc />
    public override int GetHashCode()
        => base.GetHashCode();
}
