using System;
using System.ComponentModel.DataAnnotations;
using MultiTenantApp.Application.Resources;

namespace MultiTenantApp.Application.DTOs
{
    public class ProductDto
    {
        public Guid Id { get; set; }

        [Display(Name = "Name", ResourceType = typeof(SharedResource))]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Description", ResourceType = typeof(SharedResource))]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Price", ResourceType = typeof(SharedResource))]
        public decimal Price { get; set; }
    }
}
