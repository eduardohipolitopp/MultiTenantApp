namespace MultiTenantApp.Domain.Attributes
{
    /// <summary>
    /// Attribute to mark entities that support logical (soft) delete.
    /// When applied to an entity, the IsDeleted property will be automatically set when DeleteAsync is called.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class LogicalDeleteAttribute : Attribute
    {
    }
}
