using Avalonia;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Molla.Linux;

internal static class Program
{
    [DllImport("libc", EntryPoint = "setenv", SetLastError = true)]
    private static extern int SetNativeEnvironment(string name, string value, int overwrite);

    [STAThread]
    public static void Main(string[] args)
    {
        // Native fontconfig can spin while loading caches produced by another version.
        // Isolate this application's cache without deleting or rebuilding user caches.
        if (OperatingSystem.IsLinux() && string.IsNullOrEmpty(Environment.GetEnvironmentVariable("FONTCONFIG_FILE")))
        {
            var config = Path.Combine(AppContext.BaseDirectory, "fonts.conf");
            // Fontconfig reads libc's environment, not .NET's managed environment table.
            if (SetNativeEnvironment("FONTCONFIG_FILE", config, 1) != 0)
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Unable to configure Molla fonts.");
            Environment.SetEnvironmentVariable("FONTCONFIG_FILE", config);
        }

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .LogToTrace();
}
