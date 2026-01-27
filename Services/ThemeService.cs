using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace DayLog.Services
{
    public class ThemeService
    {
        private readonly IJSRuntime _js;
        private const string KEY = "daylog:theme";

        public ThemeService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task SetDarkAsync(bool dark)
        {
            var mode = dark ? "dark" : "light";
            await _js.InvokeVoidAsync("document.documentElement.setAttribute", "data-theme", mode);
            await _js.InvokeVoidAsync("localStorage.setItem", KEY, mode);
        }

        public async Task<string?> GetSavedThemeAsync()
        {
            try
            {
                var val = await _js.InvokeAsync<string?>("localStorage.getItem", KEY);
                return val;
            }
            catch
            {
                return null;
            }
        }
    }
}
