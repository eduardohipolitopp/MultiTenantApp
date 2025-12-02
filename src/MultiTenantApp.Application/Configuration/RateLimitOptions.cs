namespace MultiTenantApp.Application.Configuration
{
    public class RateLimitOptions
    {
        public const string SectionName = "RateLimit";

        public bool Enabled { get; set; } = true;
        public GlobalRateLimitPolicy Global { get; set; } = new();
        public TenantRateLimitPolicy PerTenant { get; set; } = new();
        public UserRateLimitPolicy PerUser { get; set; } = new();
        public IpRateLimitPolicy PerIp { get; set; } = new();
        public List<EndpointRateLimitPolicy> Endpoints { get; set; } = new();
    }

    public class GlobalRateLimitPolicy
    {
        public bool Enabled { get; set; } = true;
        public int Limit { get; set; } = 1000;
        public int WindowMinutes { get; set; } = 1;
    }

    public class TenantRateLimitPolicy
    {
        public bool Enabled { get; set; } = true;
        public int Limit { get; set; } = 500;
        public int WindowMinutes { get; set; } = 1;
    }

    public class UserRateLimitPolicy
    {
        public bool Enabled { get; set; } = true;
        public int Limit { get; set; } = 100;
        public int WindowMinutes { get; set; } = 1;
    }

    public class IpRateLimitPolicy
    {
        public bool Enabled { get; set; } = true;
        public int Limit { get; set; } = 100;
        public int WindowMinutes { get; set; } = 1;
    }

    public class EndpointRateLimitPolicy
    {
        public string Endpoint { get; set; } = string.Empty;
        public int Limit { get; set; }
        public int WindowMinutes { get; set; }
        public string[] HttpMethods { get; set; } = Array.Empty<string>();
    }
}
