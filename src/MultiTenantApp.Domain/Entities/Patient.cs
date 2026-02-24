using MultiTenantApp.Domain.Attributes;
using MultiTenantApp.Domain.Common;
using MultiTenantApp.Domain.Enums;
using System;

namespace MultiTenantApp.Domain.Entities
{
    /// <summary>
    /// Represents a patient entity in the system.
    /// Supports multi-tenancy and logical delete.
    /// </summary>
    [LogicalDelete]
    public class Patient : BaseTenantEntity
    {
        /// <summary>
        /// Patient's full name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Patient's date of birth
        /// </summary>
        public DateTime BirthDate { get; set; }

        /// <summary>
        /// Patient's gender
        /// </summary>
        public Gender Gender { get; set; }

        /// <summary>
        /// Name of the guardian (optional, for minors)
        /// </summary>
        public string? GuardianName { get; set; }

        /// <summary>
        /// Contact phone number
        /// </summary>
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// Email address
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Residential address
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// Clinical or general notes
        /// </summary>
        public string? Notes { get; set; }
    }
}
