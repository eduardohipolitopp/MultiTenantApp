using System.ComponentModel.DataAnnotations;

namespace MultiTenantApp.Web.Models.Enums
{
    public enum SupportedLanguage
    {
        [Display(Name = "English")]
        EnglishUS = 0,
        
        [Display(Name = "Português")]
        PortugueseBR = 1
    }
}
