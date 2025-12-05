namespace MultiTenantApp.Web.Configuration
{
    public class CacheOptions
    {
        public const string SectionName = "Cache";

        public bool Enabled { get; set; } = true;
        public RedisOptions Redis { get; set; } = new();
        public CacheDefaults Defaults { get; set; } = new();
    }

    public class RedisOptions
    {
        public string ConnectionString { get; set; } = "localhost:6379";
        public string InstanceName { get; set; } = "MultiTenantApp:";
        public int ConnectTimeout { get; set; } = 5000;
        public int SyncTimeout { get; set; } = 5000;
        public bool AbortOnConnectFail { get; set; } = false;
        public int ConnectRetry { get; set; } = 3;
    }

    public class CacheDefaults
    {
        public int DefaultExpirationMinutes { get; set; } = 60;
        public int SlidingExpirationMinutes { get; set; } = 30;
        public bool UseDistributedCache { get; set; } = true;
    }
}
