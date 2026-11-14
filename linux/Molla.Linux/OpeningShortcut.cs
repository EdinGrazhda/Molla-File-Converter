using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Molla.Linux;

internal static class OpeningShortcut
{
    private const string Parent = "org.gnome.settings-daemon.plugins.media-keys";
    private const string Path = "/org/gnome/settings-daemon/plugins/media-keys/custom-keybindings/molla/";
    private const string Schema = Parent + ".custom-keybinding:" + Path;

    internal static async Task<string> ReadAsync()
    {
        var registered = await RunAsync("get", Parent, "custom-keybindings");
        if (!Strings(registered).Contains(Path))
            throw new InvalidOperationException("Molla’s desktop shortcut is not registered. Add it in your system Keyboard settings first.");
        return (await RunAsync("get", Schema, "binding")).Trim().Trim('\'');
    }

    internal static string Normalize(string value)
    {
        var modifiers = Regex.Matches(value, "<([^>]+)>").Select(m => m.Groups[1].Value.ToLowerInvariant())
            .Select(m => m is "ctrl" or "primary" ? "control" : m).Order().ToArray();
        return string.Join("+", modifiers) + "+" + Regex.Replace(value, "<[^>]+>", "").ToLowerInvariant();
    }

    internal static async Task SaveAsync(string binding)
    {
        await ReadAsync();
        var desired = Normalize(binding);
        var system = await RunAsync("list-recursively");
        if (Strings(system).Any(s => s.Contains('<') && Normalize(s) == desired))
            throw new InvalidOperationException("That shortcut is used by the desktop. Choose another combination.");
        var registered = await RunAsync("get", Parent, "custom-keybindings");
        foreach (var path in Strings(registered).Where(p => p != Path))
        {
            var existing = await RunAsync("get", Parent + ".custom-keybinding:" + path, "binding");
            if (Normalize(existing.Trim().Trim('\'')) == desired)
                throw new InvalidOperationException("That shortcut belongs to another app. Choose another combination.");
        }
        await RunAsync("set", Schema, "binding", binding);
        if (Normalize(await ReadAsync()) != desired)
            throw new InvalidOperationException("The desktop did not save the shortcut. Try again.");
    }

    private static IEnumerable<string> Strings(string value) =>
        Regex.Matches(value, "'([^']*)'").Select(m => m.Groups[1].Value);

    private static async Task<string> RunAsync(params string[] args)
    {
        var start = new ProcessStartInfo("gsettings")
        {
            UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true
        };
        foreach (var arg in args) start.ArgumentList.Add(arg);
        using var process = Process.Start(start) ?? throw new InvalidOperationException("Unable to open keyboard settings.");
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(8));
        try { await process.WaitForExitAsync(timeout.Token); }
        catch (OperationCanceledException)
        {
            process.Kill();
            throw new InvalidOperationException("Keyboard settings timed out. Try again.");
        }
        var details = await error;
        if (process.ExitCode != 0 || details.Contains("failed to commit", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Could not update desktop keyboard settings. " + details.Trim());
        return (await output).Trim();
    }
}
