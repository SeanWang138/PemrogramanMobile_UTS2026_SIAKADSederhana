using Microsoft.Extensions.Logging;
using Microsoft.Maui.Devices;
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

            // Base address Web API:
            // - Windows/iOS/Mac  : localhost
            // - Emulator Android : 10.0.2.2 (alias host PC, tanpa adb reverse)
            // - HP Android fisik : 127.0.0.1 lewat "adb reverse tcp:5103 tcp:5103"
            //   (127.0.0.1, bukan localhost, agar bebas ambigu resolusi IPv6 ::1
            //    yang tidak dibuka oleh adb reverse)
#if ANDROID
            var apiBaseAddress = DeviceInfo.DeviceType == DeviceType.Virtual
                ? "http://10.0.2.2:5103"
                : "http://127.0.0.1:5103";
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
