using MultiTenantApp.Domain.Enums;
using System.Globalization;

namespace MultiTenantApp.Domain.Extensions
{
    public static class SupportedLanguageExtensions
    {
        public static string ToCode(this SupportedLanguage language) => language switch
        {
            SupportedLanguage.EnglishUS => "en-US",
            SupportedLanguage.PortugueseBR => "pt-BR",
            _ => "en-US"
        };
        
        public static SupportedLanguage FromCode(string code) => code switch
        {
            "en-US" => SupportedLanguage.EnglishUS,
            "pt-BR" => SupportedLanguage.PortugueseBR,
            _ => SupportedLanguage.EnglishUS
        };
        
        public static CultureInfo ToCultureInfo(this SupportedLanguage language)
        {
            return new CultureInfo(language.ToCode());
        }
        
        public static string GetDisplayName(this SupportedLanguage language)
        {
            var field = language.GetType().GetField(language.ToString());
            var attribute = (System.ComponentModel.DataAnnotations.DisplayAttribute?)
                Attribute.GetCustomAttribute(field!, typeof(System.ComponentModel.DataAnnotations.DisplayAttribute));
            return attribute?.Name ?? language.ToString();
        }
    }
}
