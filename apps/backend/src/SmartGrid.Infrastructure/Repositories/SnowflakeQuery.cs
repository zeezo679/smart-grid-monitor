using System.Data;
using System.Data.Common;
using System.Globalization;

namespace SmartGrid.Infrastructure.Repositories;

/// <summary>
/// Shared helpers for the Snowflake repositories.
/// <list type="bullet">
/// <item>Binds use numbered placeholders (<c>:1</c>, <c>:2</c> …), the form the Snowflake .NET
/// connector documents, so dynamic WHERE clauses stay parameterised.</item>
/// <item>MARTS numbers are FLOAT, so values are converted instead of read with GetDecimal.</item>
/// <item>MARTS timestamps are TIMESTAMP_NTZ in UTC, so they are tagged as UTC.</item>
/// </list>
/// </summary>
internal sealed class SnowflakeQuery
{
    private readonly DbCommand _command;
    private int _nextIndex = 1;

    public SnowflakeQuery(DbCommand command)
    {
        _command = command ?? throw new ArgumentNullException(nameof(command));
    }

    /// <summary>Adds a bind value and returns its placeholder (e.g. <c>:3</c>) for the SQL text.</summary>
    public string Bind(object value, DbType dbType)
    {
        var name = _nextIndex.ToString(CultureInfo.InvariantCulture);
        _nextIndex++;

        var param = _command.CreateParameter();
        param.ParameterName = name;
        param.DbType = dbType;
        param.Value = value;
        _command.Parameters.Add(param);

        return ":" + name;
    }

    public string BindString(string value) => Bind(value, DbType.String);

    public string BindDateTime(DateTime value) => Bind(value, DbType.DateTime);

    public static decimal ReadDecimal(DbDataReader reader, int ordinal)
        => Convert.ToDecimal(reader.GetValue(ordinal), CultureInfo.InvariantCulture);

    public static decimal? ReadNullableDecimal(DbDataReader reader, int ordinal)
        => reader.IsDBNull(ordinal) ? null : ReadDecimal(reader, ordinal);

    public static int ReadInt(DbDataReader reader, int ordinal)
        => reader.IsDBNull(ordinal) ? 0 : Convert.ToInt32(reader.GetValue(ordinal), CultureInfo.InvariantCulture);

    public static int? ReadNullableInt(DbDataReader reader, int ordinal)
        => reader.IsDBNull(ordinal) ? null : Convert.ToInt32(reader.GetValue(ordinal), CultureInfo.InvariantCulture);

    public static string ReadString(DbDataReader reader, int ordinal, string fallback = "")
        => reader.IsDBNull(ordinal) ? fallback : Convert.ToString(reader.GetValue(ordinal), CultureInfo.InvariantCulture) ?? fallback;

    public static double ReadDouble(DbDataReader reader, int ordinal)
        => reader.IsDBNull(ordinal) ? double.NaN : Convert.ToDouble(reader.GetValue(ordinal), CultureInfo.InvariantCulture);

    public static DateTimeOffset ReadUtc(DbDataReader reader, int ordinal)
        => new(DateTime.SpecifyKind(reader.GetDateTime(ordinal), DateTimeKind.Utc));

    public static DateTimeOffset? ReadNullableUtc(DbDataReader reader, int ordinal)
        => reader.IsDBNull(ordinal) ? null : ReadUtc(reader, ordinal);

    /// <summary>Inclusive start of the first day, bound the same way as the existing map query (TIMESTAMP_NTZ).</summary>
    public static DateTime StartOf(DateOnly day) => day.ToDateTime(TimeOnly.MinValue);

    /// <summary>Exclusive end: midnight after the last day.</summary>
    public static DateTime EndExclusiveOf(DateOnly day) => day.AddDays(1).ToDateTime(TimeOnly.MinValue);

    /// <summary>LIMIT/OFFSET text. Both come from a validated PageRequest (ints), so inlining is safe.</summary>
    public static string LimitOffset(int page, int pageSize)
        => string.Create(CultureInfo.InvariantCulture, $" LIMIT {pageSize} OFFSET {(page - 1) * pageSize}");
}
