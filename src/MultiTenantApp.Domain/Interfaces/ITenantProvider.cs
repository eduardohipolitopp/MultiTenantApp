namespace MultiTenantApp.Domain.Interfaces
{
    public interface ITenantProvider
    {
        string? GetTenantId();
        void SetTenantId(string tenantId);
    }
}
