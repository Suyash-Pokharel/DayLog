using Microsoft.AspNetCore.Components.WebView.Maui;

namespace DayLog
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            this.blazorWebView.RootComponents.Clear();
            this.blazorWebView.RootComponents.Add(new RootComponent
            {
                Selector = "#app",
                ComponentType = typeof(Components.Routes)
            });
        }
    }
}
