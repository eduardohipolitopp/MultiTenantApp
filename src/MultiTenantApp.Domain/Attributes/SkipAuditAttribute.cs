using System;

namespace MultiTenantApp.Domain.Attributes
{
    /// <summary>
    /// Attribute to mark entities that should not be audited.
    /// Apply this to entity classes to exclude them from audit logging.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class SkipAuditAttribute : Attribute
    {
    }
}
