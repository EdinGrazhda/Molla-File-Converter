using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Avalonia.Input;
using ImageMagick;
using Molla.Linux;

internal static class UiSmoke
{
    public static void Run()
    {
        AppBuilder.Configure<App>().UsePlatformDetect().AfterSetup(_ =>
            Dispatcher.UIThread.Post(async () =>
            {
                try
                {
                    await Task.Delay(800);
                    var lifetime = (IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!;
                    var window = (AppleWindow)lifetime.MainWindow!;
                    if (!window.IsVisible) throw new Exception("Apple window is not visible");
                    foreach (var name in new[] { "ColorButton", "CollapseButton", "CloseButton" })
                    {
                        var button = window.FindControl<Button>(name)!;
                        foreach (var local in new[] { new Point(1, 1), new Point(49, 1), new Point(25, 25), new Point(1, 49), new Point(49, 49) })
                        {
                            var point = button.TranslatePoint(local, window) ?? throw new Exception("Missing button transform");
                            var hit = window.InputHitTest(point) as Visual;
                            if (hit != button && (hit is null || !hit.GetVisualAncestors().Contains(button)))
                                throw new Exception(name + " does not own its full hit area");
                        }
                    }
                    Console.WriteLine("PASS: Header buttons own center and corner click targets; no drag interception");
                    foreach (var label in window.GetVisualDescendants().OfType<TextBlock>().Where(t => t.Text is "molla" or "+ Add files" or "PNG"))
                    {
                        var face = new Avalonia.Media.Typeface(label.FontFamily, label.FontStyle, label.FontWeight).GlyphTypeface;
                        Console.WriteLine($"Font {label.Text}: {label.FontFamily} / {label.FontWeight} / {face.FamilyName}");
                    }
                    using (var bitmap = new RenderTargetBitmap(new PixelSize(400, 456)))
                    {
                        bitmap.Render(window);
                        bitmap.Save("/tmp/molla-apple-preview.png", PngBitmapEncoderOptions.Default);
                    }
                    Console.WriteLine("PASS: Apple window renders; preview /tmp/molla-apple-preview.png");
                    Button Find(string label) => window.GetVisualDescendants().OfType<Button>().First(b => b.Content as string == label);
                    Find("Audio").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    if (window.FindControl<Button>("Slot0")!.Content as string != "MP3") throw new Exception("Audio format switch failed");
                    Find("Images").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Find("−").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    if (window.Width != 110) throw new Exception("Collapse failed");
                    Find("Molla").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    if (window.Width != 400) throw new Exception("Expand failed");
                    Console.WriteLine("PASS: Category, collapse and expand controls");
                    var settingsButton = window.FindControl<Button>("ColorButton")!;
                    settingsButton.Flyout!.ShowAt(settingsButton);
                    window.FindControl<TabItem>("ShortcutTab")!.IsSelected = true;
                    var saveShortcut = window.FindControl<Button>("SaveShortcutButton")!;
                    for (var i = 0; i < 100 && !saveShortcut.IsEnabled; i++) await Task.Delay(100);
                    if (!saveShortcut.IsEnabled) throw new Exception("Shortcut editor did not load desktop settings");
                    var currentShortcut = window.FindControl<TextBlock>("CurrentShortcut")!.Text;
                    // Save the existing combination unchanged, never replace the user's shortcut in a test.
                    saveShortcut.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    for (var i = 0; i < 300 && !saveShortcut.IsEnabled; i++) await Task.Delay(100);
                    if (!window.FindControl<TextBlock>("ShortcutFeedback")!.Text!.StartsWith("Saved."))
                        throw new Exception("Shortcut save failed: " + window.FindControl<TextBlock>("ShortcutFeedback")!.Text);
                    await Task.Delay(100);
                    using (var capture = new RenderTargetBitmap(new PixelSize(354, 330)))
                    {
                        var tabs = (Control)settingsButton.Flyout.GetType().GetProperty("Content")!.GetValue(settingsButton.Flyout)!;
                        capture.Render(tabs);
                        capture.Save("/tmp/molla-shortcut-editor.png", PngBitmapEncoderOptions.Default);
                    }
                    window.FindControl<CheckBox>("ShortcutCtrl")!.IsChecked = false;
                    window.FindControl<CheckBox>("ShortcutAlt")!.IsChecked = true;
                    window.FindControl<CheckBox>("ShortcutShift")!.IsChecked = false;
                    window.FindControl<CheckBox>("ShortcutSuper")!.IsChecked = false;
                    window.FindControl<ComboBox>("ShortcutKey")!.SelectedItem = "F4";
                    saveShortcut.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    for (var i = 0; i < 300 && !saveShortcut.IsEnabled; i++) await Task.Delay(100);
                    if (!window.FindControl<TextBlock>("ShortcutFeedback")!.Text!.Contains("used by the desktop") ||
                        window.FindControl<TextBlock>("CurrentShortcut")!.Text != currentShortcut)
                        throw new Exception("Conflicting shortcut was not rejected");
                    settingsButton.Flyout.Hide();
                    Console.WriteLine("PASS: Shortcut loads, saves unchanged, and rejects Alt+F4 conflict");
                    foreach (var color in new[] { "Red", "Green", "Yellow", "Gray" })
                    {
                        window.FindControl<Button>("ColorButton")!.Flyout!.ShowAt(window.FindControl<Button>("ColorButton")!);
                        await Task.Delay(80);
                        window.FindControl<Button>(color + "Choice")!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        var full = (Avalonia.Media.LinearGradientBrush)window.FindControl<Avalonia.Controls.Shapes.Path>("AppleBody")!.Fill!;
                        var mini = window.FindControl<Avalonia.Controls.Shapes.Path>("MiniBody")!.Fill;
                        if (!ReferenceEquals(full, mini)) throw new Exception("Apple colors diverged");
                        if (full.GradientStops[0].Color.A != 217 || full.GradientStops[1].Color.A != 179 || full.GradientStops[2].Color.A != 204)
                            throw new Exception("Glass opacity changed");
                        await Task.Delay(80);
                        using (var capture = new RenderTargetBitmap(new PixelSize(400, 456)))
                        {
                            capture.Render(window);
                            capture.Save("/tmp/molla-" + color.ToLowerInvariant() + ".png", PngBitmapEncoderOptions.Default);
                        }
                        Find("−").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        await Task.Delay(80);
                        using (var capture = new RenderTargetBitmap(new PixelSize(110, 135)))
                        {
                            capture.Render(window);
                            capture.Save("/tmp/molla-mini-" + color.ToLowerInvariant() + ".png", PngBitmapEncoderOptions.Default);
                        }
                        Find("Molla").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    }
                    Console.WriteLine("PASS: Red, green, yellow, gray synchronize both apples with unchanged glass opacity");
                    var source = Path.Combine(Path.GetTempPath(), "molla-ui-test-" + Guid.NewGuid().ToString("N") + ".png");
                    using (var sample = new MagickImage(MagickColors.Coral, 16, 16)) sample.Write(source);
                    var file = await window.StorageProvider.TryGetFileFromPathAsync(new Uri(source)) ?? throw new Exception("Cannot expose local sample to drag/drop");
                    using var transfer = new DataTransfer();
                    transfer.Add(DataTransferItem.CreateFile(file));
                    Find("−").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    var launcher = window.FindControl<Grid>("Collapsed")!;
                    launcher.RaiseEvent(new DragEventArgs(DragDrop.DropEvent, transfer, launcher, new Point(50, 65), KeyModifiers.None));
                    if (window.Width != 400 || window.FindControl<TextBlock>("SelectionLabel")!.Text != "1 file ready")
                        throw new Exception("Dropping on launcher did not expand and select file");
                    Console.WriteLine("PASS: Drop on mini apple opens converter with file selected");
                    var target = window.FindControl<Button>("Slot1")!;
                    var over = new DragEventArgs(DragDrop.DragOverEvent, transfer, target, new Point(10, 10), KeyModifiers.None);
                    target.RaiseEvent(over);
                    if (over.DragEffects != DragDropEffects.Copy) throw new Exception("PNG → JPG drop rejected");
                    target.RaiseEvent(new DragEventArgs(DragDrop.DropEvent, transfer, target, new Point(10, 10), KeyModifiers.None));
                    var result = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "Molla", Path.GetFileNameWithoutExtension(source) + ".jpg");
                    for (var i = 0; i < 100 && window.FindControl<Button>("CancelButton")!.IsVisible; i++) await Task.Delay(100);
                    using (var converted = new MagickImage(result))
                        if (converted.Width != 16 || converted.Height != 16) throw new Exception("Drop produced wrong image");
                    Console.WriteLine("PASS: File drag-over → JPG drop → real conversion and output");
                    // Remove only this test's unique generated output, never user files.
                    File.Delete(result);
                    File.Delete(source);
                    lifetime.Shutdown();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                    ((IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!).Shutdown(1);
                    Environment.ExitCode = 1;
                }
            })).StartWithClassicDesktopLifetime(["--apple"]);
    }
}
