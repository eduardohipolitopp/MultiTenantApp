namespace MultiTenantApp.Api.Attributes
{
    /// <summary>
    /// Attribute to disable rate limiting for specific endpoints
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class DisableRateLimitAttribute : Attribute
    {
    }
}
