using System.Collections.Generic;

namespace MultiTenantApp.Observability;

public class ObservabilityOptions
{
    public const string SectionName = "Observability";

    public string? ServiceName { get; set; }
    public string? ServiceVersion { get; set; }
    public string? Environment { get; set; }
    public string? OtlpEndpoint { get; set; }
    
    public bool EnableTracing { get; set; } = true;
    public bool EnableMetrics { get; set; } = true;
    public bool EnableLogging { get; set; } = true;

    public ConsoleOptions Console { get; set; } = new();
    
    public Dictionary<string, object>? ResourceAttributes { get; set; }

    public class ConsoleOptions
    {
        public bool Enabled { get; set; } = true;
        /// <summary>
        /// When true, logs are written as JSON (one object per line) so Grafana/Loki can filter by level and other fields.
        /// When false, uses OutputTemplate for human-readable text.
        /// </summary>
        public bool UseJsonFormat { get; set; } = true;
        public string? OutputTemplate { get; set; }
    }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(OtlpEndpoint) && (EnableTracing || EnableMetrics || EnableLogging))
        {
            throw new ArgumentException("OtlpEndpoint is required when Tracing, Metrics, or Logging are enabled.", nameof(OtlpEndpoint));
        }
    }
}
