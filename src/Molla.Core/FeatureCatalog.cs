namespace Molla.Core;

public static class FeatureCatalog
{
    public static string Extension(string path) => Path.GetExtension(path).TrimStart('.').ToLowerInvariant() switch
    {
        "jpeg" => "jpg",
        "tif" => "tiff",
        "heif" => "heic",
        "m4v" => "mp4",
        var extension => extension
    };

    public static bool IsImage(string path) =>
        new[] { "jpg", "png", "webp", "heic", "tiff", "svg", "avif", "bmp" }.Contains(Extension(path));

    public static bool IsVideo(string path) =>
        new[] { "mp4", "mov", "mkv", "webm", "avi", "wmv", "gif" }.Contains(Extension(path));

    public static bool IsAudio(string path) =>
        new[] { "mp3", "m4a", "wav", "flac", "ogg", "opus", "aiff", "wma", "aac" }.Contains(Extension(path));
}
