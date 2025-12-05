using System.ComponentModel.DataAnnotations;

namespace MultiTenantApp.Web.Models.DTOs
{
    public class UpdateProductDto
    {
        [Display(Name = "Name")]
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Price")]
        public decimal Price { get; set; }
    }
}
