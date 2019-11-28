using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Logging.Serilog;
using Avalonia.OpenGL;

namespace AvaloniaApplication2
{
    class Program
    {
        public static void Main(string[] args) => BuildAvaloniaApp().Start(AppMain, args);

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .With(new Win32PlatformOptions {AllowEglInitialization = true})
                .With(new X11PlatformOptions {UseGpu = true, UseEGL = true})
                .With(new AvaloniaNativePlatformOptions {UseGpu = true})
                .With(new AngleOptions
                    {AllowedPlatformApis = new List<AngleOptions.PlatformApi> {AngleOptions.PlatformApi.DirectX11}})
                .LogToDebug();

        private static void AppMain(Application app, string[] args)
        {
            app.Run(new MainWindow());
        }
    }
}
