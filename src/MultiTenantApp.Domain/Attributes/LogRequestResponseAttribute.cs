namespace MultiTenantApp.Domain.Attributes
{
    /// <summary>
    /// Attribute to mark endpoints that should log request/response to MongoDB.
    /// When applied to a controller or action, all requests and responses will be logged.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class LogRequestResponseAttribute : Attribute
    {
        /// <summary>
        /// Whether to log the request body. Default is true.
        /// </summary>
        public bool LogRequestBody { get; set; } = true;

        /// <summary>
        /// Whether to log the response body. Default is true.
        /// </summary>
        public bool LogResponseBody { get; set; } = true;

        /// <summary>
        /// Maximum length of request/response body to log. Default is 10000 characters.
        /// </summary>
        public int MaxBodyLength { get; set; } = 10000;
    }
}
