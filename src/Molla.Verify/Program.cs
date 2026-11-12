using ImageMagick;
using Molla.Linux;
using Molla.Core;
using System.Security.Cryptography;

if (args.Contains("--ui"))
{
    UiSmoke.Run();
    return;
}

var root = Directory.CreateTempSubdirectory("molla-verify-").FullName;
Console.WriteLine("Test files: " + root);
var source = Path.Combine(root, "original.png");
using (var image = new MagickImage(MagickColors.Coral, 24, 24)) image.Write(source);
var originalHash = SHA256.HashData(File.ReadAllBytes(source));
var converter = new QuickConverter();
var output = Path.Combine(root, "output");
var checks = 0;
void Check(bool valid, string name)
{
    if (!valid) throw new Exception("FAIL: " + name);
    Console.WriteLine("PASS: " + name); checks++;
}

foreach (var format in QuickConverter.ImageFormats)
{
    var files = await converter.ConvertAsync([source], format, output, null, default);
    using var image = new MagickImage(files.Single());
    Check(image.Width == 24 && image.Height == 24, "Image → " + format);
}
var duplicate = await converter.ConvertAsync([source], "jpg", output, null, default);
Check(Path.GetFileName(duplicate.Single()) != "original.jpg" && File.Exists(Path.Combine(output, "original.jpg")), "Existing output preserved; unique filename");
Check(originalHash.SequenceEqual(SHA256.HashData(File.ReadAllBytes(source))), "Original bytes unchanged");
Check(!converter.CanConvert([source], "mp3"), "Image/audio mismatch rejected");
Check(!converter.CanConvert([root], "png"), "Directories rejected");
Check(!converter.CanConvert([], "png"), "Empty input rejected");
var cancelledOutput = Path.Combine(root, "cancelled");
try
{
    await converter.ConvertAsync([source], "jpg", cancelledOutput, null, new CancellationToken(true));
    throw new Exception("Cancellation was ignored");
}
catch (OperationCanceledException) { Check(!Directory.Exists(cancelledOutput), "Cancellation publishes nothing"); }
var broken = Path.Combine(root, "broken.png");
await File.WriteAllTextAsync(broken, "not an image");
var failedOutput = Path.Combine(root, "failed");
try
{
    await converter.ConvertAsync([source, broken], "jpg", failedOutput, null, default);
    throw new Exception("Corrupt input was accepted");
}
catch (MagickException) { Check(!Directory.EnumerateFileSystemEntries(failedOutput).Any(), "Failed batch rolls back all staged images"); }

if (converter.MediaAvailable)
{
    var wav = Path.Combine(root, "tone.wav");
    await ProcessRunner.RunAsync("/usr/bin/ffmpeg", ["-v", "error", "-f", "lavfi", "-i", "sine=frequency=440:duration=0.2", wav]);
    foreach (var format in QuickConverter.AudioFormats)
    {
        var files = await converter.ConvertAsync([wav], format, output, null, default);
        var info = await new MediaEngine("/usr/bin/ffmpeg", "/usr/bin/ffprobe").InspectAsync(files.Single());
        Check(info.HasAudio && info.Duration > 0, "Audio → " + format);
    }
    var video = Path.Combine(root, "clip.mp4");
    await ProcessRunner.RunAsync("/usr/bin/ffmpeg", ["-v", "error", "-f", "lavfi", "-i", "color=c=green:s=32x32:d=0.2", "-c:v", "libx264", video]);
    foreach (var format in QuickConverter.VideoFormats)
    {
        var files = await converter.ConvertAsync([video], format, output, null, default);
        var info = await new MediaEngine("/usr/bin/ffmpeg", "/usr/bin/ffprobe").InspectAsync(files.Single());
        Check(info.Video is not null, "Video → " + format);
    }
}
else Console.WriteLine("SKIP: media tools unavailable");
Console.WriteLine($"{checks} checks passed.");
