namespace MultiTenantApp.Web.Services
{
    public interface ITokenProvider
    {
        string Token { get; }
        event EventHandler TokenChanged;
        Task SetTokenAsync(string token);
        Task ClearTokenAsync();
        /// <summary>Um Task que completa quando o token for definido (ou nulo se limpo).</summary>
        Task WaitForTokenAsync(CancellationToken cancellationToken = default);
    }

}
