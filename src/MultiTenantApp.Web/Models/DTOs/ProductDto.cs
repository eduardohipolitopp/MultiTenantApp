using System;
using System.ComponentModel.DataAnnotations;

namespace MultiTenantApp.Web.Models.DTOs
{
    public class ProductDto
    {
        public Guid Id { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Price")]
        public decimal Price { get; set; }
    }
}
