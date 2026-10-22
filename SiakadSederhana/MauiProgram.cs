using Microsoft.Extensions.Logging;
using SiakadSederhana.Services;

namespace SiakadSederhana
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            // Base address Web API.
            // Emulator Android mengakses komputer host melalui 10.0.2.2,
            // platform lain (Windows/iOS/Mac) cukup memakai localhost.
#if ANDROID
            var apiBaseAddress = "http://10.0.2.2:5103";
#else
            var apiBaseAddress = "http://localhost:5103";
#endif
            builder.Services.AddSingleton(_ => new HttpClient
            {
                BaseAddress = new Uri(apiBaseAddress),
                Timeout = TimeSpan.FromSeconds(15)
            });

            builder.Services.AddSingleton<DosenService>();
            builder.Services.AddSingleton<MahasiswaService>();
            builder.Services.AddSingleton<ToastService>();

            return builder.Build();
        }
    }
}
