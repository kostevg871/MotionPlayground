using Microsoft.Extensions.Logging;

#if ANDROID
using Microsoft.Maui.Handlers;
#endif

namespace MotionPlayground
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
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

#if ANDROID
            WebViewHandler.Mapper.AppendToMapping("EnableJs", (handler, view) =>
            {
                var s = handler.PlatformView.Settings;
                s.JavaScriptEnabled = true;
            });
#endif

            return builder.Build();
        }
    }
}
