using Blazored.LocalStorage;
using MudBlazor;

namespace MultiTenantApp.Web.Services
{
    public class ThemeService
    {
        private readonly ILocalStorageService _localStorage;
        private const string ThemeKey = "user_theme_preference";

        public MudTheme CurrentTheme { get; private set; } = new MudTheme();
        public bool IsDarkMode { get; private set; } = false;
        
        public event Action? OnThemeChanged;

        public ThemeService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
            SetTheme("Blue"); // Default
        }

        public async Task InitializeAsync()
        {
            try
            {
                var savedTheme = await _localStorage.GetItemAsync<string>(ThemeKey);
                if (!string.IsNullOrEmpty(savedTheme))
                {
                    var parts = savedTheme.Split('_');
                    if (parts.Length == 2)
                    {
                        SetTheme(parts[0]);
                        IsDarkMode = parts[1] == "Dark";
                        OnThemeChanged?.Invoke();
                    }
                }
            }
            catch
            {
                // Ignore errors reading local storage
            }
        }

        public async Task SetThemeAsync(string color, bool isDark)
        {
            SetTheme(color);
            IsDarkMode = isDark;
            OnThemeChanged?.Invoke();
            await _localStorage.SetItemAsync(ThemeKey, $"{color}_{(isDark ? "Dark" : "Light")}");
        }

        private void SetTheme(string color)
        {
            var primaryColor = color switch
            {
                "Blue" => Colors.Blue.Default,
                "Red" => Colors.Red.Default,
                "Green" => Colors.Green.Default,
                "Pink" => Colors.Pink.Default,
                _ => Colors.Blue.Default
            };

            CurrentTheme = new MudTheme
            {
                Palette = new PaletteLight
                {
                    Primary = primaryColor,
                    AppbarBackground = primaryColor,
                },
                PaletteDark = new PaletteDark
                {
                    Primary = primaryColor,
                    AppbarBackground = primaryColor, // Usually dark appbar in dark mode
                }
            };
        }
    }
}
