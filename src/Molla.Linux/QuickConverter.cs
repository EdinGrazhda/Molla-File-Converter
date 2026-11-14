using Molla.Core;

namespace Molla.Linux;

/// <summary>The same safe conversion path is used by the picker and desktop drops.</summary>
public sealed class QuickConverter
{
    public static readonly string[] ImageFormats = ["png", "jpg", "webp", "avif", "tiff", "bmp"];
    public static readonly string[] AudioFormats = ["mp3", "wav", "flac", "m4a", "ogg", "opus"];
    public static readonly string[] VideoFormats = ["mp4", "mov", "mkv", "webm", "avi", "wmv"];
    private readonly string? ffmpeg = FindExecutable("ffmpeg");
    private readonly string? ffprobe = FindExecutable("ffprobe");
    public bool MediaAvailable => ffmpeg is not null && ffprobe is not null;

    public bool CanConvert(IReadOnlyList<string> files, string format)
    {
        if (files.Count == 0 || files.Any(p => !File.Exists(p))) return false;
        if (ImageFormats.Contains(format) && files.All(FeatureCatalog.IsImage)) return true;
        return MediaAvailable && ((AudioFormats.Contains(format) && files.All(p => FeatureCatalog.IsAudio(p) || FeatureCatalog.IsVideo(p)))
            || (VideoFormats.Contains(format) && files.All(FeatureCatalog.IsVideo)));
    }

    public async Task<IReadOnlyList<string>> ConvertAsync(IReadOnlyList<string> files, string format,
        string destination, IProgress<OperationProgress>? progress, CancellationToken token)
    {
        if (!CanConvert(files, format))
            throw new ArgumentException("These files cannot use that format. Choose Images, Audio, or Video to match your files.");
        token.ThrowIfCancellationRequested();
        if (files.All(FeatureCatalog.IsImage))
        {
            return await Task.Run<IReadOnlyList<string>>(() =>
            {
                using var transaction = new ExportTransaction(destination);
                var outputs = new List<string>();
                for (var i = 0; i < files.Count; i++)
                {
                    token.ThrowIfCancellationRequested();
                    progress?.Report(new((double)i / files.Count, $"Converting {i + 1} of {files.Count}…"));
                    var output = SafePaths.Unique(transaction.StagingDirectory, Path.GetFileNameWithoutExtension(files[i]) + "." + format);
                    PhotoCodec.EncodePng(PhotoCodec.Png(files[i]), output, format, token: token);
                    outputs.Add(output);
                }
                return transaction.Publish(outputs, token);
            }, token);
        }
        return await new MediaEngine(ffmpeg!, ffprobe!).ConvertAsync(files, format, destination,
            new MediaOptions(), progress, token);
    }

    private static string? FindExecutable(string name) =>
        (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator)
        .Where(p => Path.IsPathFullyQualified(p)).Select(p => Path.Combine(p, name))
        .FirstOrDefault(p => File.Exists(p) && !OperatingSystem.IsWindows() && (File.GetUnixFileMode(p) &
            (UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute)) != 0);
}
