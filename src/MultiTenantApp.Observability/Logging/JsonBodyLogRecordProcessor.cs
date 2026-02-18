using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;

namespace MultiTenantApp.Observability.Logging;

/// <summary>
/// Sets the log record Body to a single-line JSON string so Loki stores it in the "line" field
/// and Grafana can parse it with `| json` to filter by level (@l) and other properties.
/// </summary>
public sealed class JsonBodyLogRecordProcessor : BaseProcessor<LogRecord>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public override void OnEnd(LogRecord record)
    {
        if (record == null) return;

        var obj = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["@l"] = GetSeverityText(record.LogLevel),
            ["@m"] = record.Body ?? record.FormattedMessage ?? string.Empty,
            ["@t"] = record.Timestamp.ToString("O")
        };

        if (record.Attributes != null)
        {
            foreach (var kv in record.Attributes)
            {
                if (string.IsNullOrEmpty(kv.Key)) continue;
                var key = kv.Key == "{OriginalFormat}" ? "@mt" : kv.Key;
                obj[key] = ToJsonValue(kv.Value);
            }
        }

        if (!string.IsNullOrEmpty(record.CategoryName))
            obj["CategoryName"] = record.CategoryName;

        if (record.EventId.Id != 0 || !string.IsNullOrEmpty(record.EventId.Name))
            obj["EventId"] = $"{record.EventId.Id}:{record.EventId.Name}";

        if (record.Exception != null)
            obj["@x"] = record.Exception.ToString();

        record.Body = JsonSerializer.Serialize(obj, JsonOptions);
    }

    private static string GetSeverityText(LogLevel level) => level switch
    {
        LogLevel.Trace => "Trace",
        LogLevel.Debug => "Debug",
        LogLevel.Information => "Information",
        LogLevel.Warning => "Warning",
        LogLevel.Error => "Error",
        LogLevel.Critical => "Critical",
        LogLevel.None => "None",
        _ => "Information"
    };

    private static object? ToJsonValue(object? value)
    {
        if (value == null) return null;
        if (value is string s) return s;
        if (value is int or long or short or byte) return value;
        if (value is uint or ulong or ushort) return value;
        if (value is float or double or decimal) return value;
        if (value is bool) return value;
        if (value is System.DateTime dt) return dt.ToString("O");
        if (value is System.DateTimeOffset dto) return dto.ToString("O");
        return value.ToString();
    }
}
