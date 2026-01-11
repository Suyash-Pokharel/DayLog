using Microsoft.Maui.Controls;
using Microsoft.AspNetCore.Components.WebView.Maui;

namespace DayLog
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            // Ensure the root component for the BlazorWebView is registered
            // 'Components.Routes' is the Razor component containing the Router
            this.blazorWebView.RootComponents.Clear();
            this.blazorWebView.RootComponents.Add(new RootComponent
            {
                Selector = "#app",
                ComponentType = typeof(Components.Routes)
            });
        }
    }
}
