using System.Collections.Generic;

namespace NativeData.Abstractions;

/// <summary>
/// An AOT-safe predicate for filtering entities in a NativeData query.
/// Contains a raw SQL WHERE clause fragment and its associated parameters.
/// </summary>
/// <param name="Sql">SQL WHERE clause body (without the <c>WHERE</c> keyword).</param>
/// <param name="Parameters">Named parameters referenced in <see cref="Sql"/>.</param>
public readonly record struct QueryFilter(string Sql, IReadOnlyList<SqlParameterValue>? Parameters = null);
