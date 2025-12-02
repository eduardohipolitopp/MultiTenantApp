namespace MultiTenantApp.Domain.Interfaces
{
    public interface ITenantProvider
    {
        Guid? GetTenantId();
        void SetTenantId(Guid tenantId);
    }
}
