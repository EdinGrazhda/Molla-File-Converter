using Avalonia.Controls;
using Avalonia;
using Avalonia.Media;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Molla.Core;

namespace Molla.Linux;

public sealed partial class AppleWindow : Window
{
    private readonly QuickConverter converter = new();
    private readonly string destination = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "Molla");
    private string[] selected = [];
    private CancellationTokenSource? operation;
    private readonly Button[] slots;
    private PointerPressedEventArgs? miniPress;
    private Point miniPressPoint;
    private bool miniDragging;

    public AppleWindow()
    {
        InitializeComponent();
        ShortcutKey.ItemsSource = Enumerable.Range('A', 26).Select(c => ((char)c).ToString())
            .Concat(Enumerable.Range(1, 12).Select(i => "F" + i)).ToArray();
        ColorButton.Flyout!.Opened += async (_, _) => await LoadOpeningShortcut();
        slots = [Slot0, Slot1, Slot2, Slot3, Slot4, Slot5];
        foreach (var button in slots)
        {
            DragDrop.SetAllowDrop(button, true);
            button.AddHandler(DragDrop.DragOverEvent, OnDragOver);
            button.AddHandler(DragDrop.DragLeaveEvent, (_, _) => button.Classes.Remove("drag-over"));
            button.AddHandler(DragDrop.DropEvent, OnDrop);
            button.Click += ConvertSelected;
        }
        DropCenter.AddHandler(DragDrop.DragOverEvent, OnDragOver);
        DropCenter.AddHandler(DragDrop.DropEvent, OnDrop);
        Collapsed.AddHandler(DragDrop.DragOverEvent, OnDragOver);
        Collapsed.AddHandler(DragDrop.DropEvent, OnDrop);
        Collapsed.AddHandler(PointerPressedEvent, MiniPressed, RoutingStrategies.Tunnel, true);
        Collapsed.AddHandler(PointerMovedEvent, MiniMoved, RoutingStrategies.Tunnel, true);
        Collapsed.AddHandler(PointerReleasedEvent, (_, _) => miniPress = null, RoutingStrategies.Tunnel, true);
        SetCategory("images");
        Closing += (_, e) =>
        {
            if (operation is null) return;
            e.Cancel = true;
            operation.Cancel();
            Status.Text = "Cancelling… Close again when finished.";
        };
        Opened += (_, _) => Console.WriteLine($"Molla desktop apple opened. Transparency: {ActualTransparencyLevel}");
    }

    private static string[] Files(DragEventArgs e) => e.DataTransfer.TryGetFiles()?
        .OfType<IStorageFile>().Select(f => f.TryGetLocalPath()).OfType<string>()
        .Distinct(StringComparer.Ordinal).ToArray() ?? [];

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        var files = Files(e);
        var allowed = operation is null && files.Length > 0 &&
            (sender is not Button button || button.Tag is not string format || converter.CanConvert(files, format));
        e.DragEffects = allowed ? DragDropEffects.Copy : DragDropEffects.None;
        if (sender is Button target) target.Classes.Set("drag-over", allowed);
        e.Handled = true;
    }

    private async void OnDrop(object? sender, DragEventArgs e)
    {
        e.Handled = true;
        if (sender is Button target) target.Classes.Remove("drag-over");
        if (operation is not null) return;
        var files = Files(e);
        if (files.Length == 0) { Status.Text = "Drop local files, not folders or links."; return; }
        if (sender is Button button && button.Tag is string format)
            await ConvertFiles(files, format);
        else
        {
            if (Collapsed.IsVisible) ShowExpanded();
            SetSelection(files);
        }
    }

    private void SetSelection(string[] files)
    {
        selected = files;
        SelectionLabel.Text = files.Length == 1 ? "1 file ready" : $"{files.Length} files ready";
        SetCategory(files.All(FeatureCatalog.IsImage) ? "images" : files.All(FeatureCatalog.IsAudio) ? "audio" : "video");
        Status.Text = "Choose a format. Originals stay untouched.";
    }

    private async void ChooseFiles(object? sender, RoutedEventArgs e)
    {
        if (operation is not null) return;
        try
        {
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions { Title = "Choose files to convert", AllowMultiple = true });
            var paths = files.Select(f => f.TryGetLocalPath()).OfType<string>().ToArray();
            if (paths.Length > 0) SetSelection(paths);
        }
        catch (Exception ex) { Status.Text = ex.Message; }
    }

    private void ChangeCategory(object? sender, RoutedEventArgs e)
    {
        if (operation is not null) return;
        if (sender is Button { Tag: string category }) SetCategory(category);
    }

    private void SetCategory(string category)
    {
        var formats = category switch { "audio" => QuickConverter.AudioFormats, "video" => QuickConverter.VideoFormats, _ => QuickConverter.ImageFormats };
        CategoryLabel.Text = category.ToUpperInvariant();
        ImagesTab.Classes.Set("active", category == "images");
        AudioTab.Classes.Set("active", category == "audio");
        VideoTab.Classes.Set("active", category == "video");
        for (var i = 0; i < slots.Length; i++)
        {
            slots[i].Content = formats[i].ToUpperInvariant();
            slots[i].Tag = formats[i];
            slots[i].IsEnabled = operation is null && (category == "images" || converter.MediaAvailable);
            ToolTip.SetTip(slots[i], "Drop files here, or choose files then click. Saves to ~/Downloads/Molla.");
        }
        if (category != "images" && !converter.MediaAvailable)
            Status.Text = "Audio/video need FFmpeg and FFprobe on PATH. Nothing was installed.";
    }

    private async void ConvertSelected(object? sender, RoutedEventArgs e)
    {
        if (selected.Length == 0) { Status.Text = "Drop files onto a format, or choose files in the center."; return; }
        if (sender is Button { Tag: string format }) await ConvertFiles(selected.ToArray(), format);
    }

    private async Task ConvertFiles(string[] files, string format)
    {
        if (operation is not null) return;
        operation = new CancellationTokenSource();
        CancelButton.IsVisible = true;
        ChooseButton.IsEnabled = false;
        foreach (var slot in slots) slot.IsEnabled = false;
        Status.Text = $"Converting to {format.ToUpperInvariant()}…";
        try
        {
            var progress = new Progress<OperationProgress>(p => Status.Text = p.Label);
            var outputs = await converter.ConvertAsync(files, format, destination, progress, operation.Token);
            Status.Text = $"Saved {outputs.Count} file(s) to Downloads/Molla. Originals kept.";
        }
        catch (OperationCanceledException) { Status.Text = "Cancelled. No converted files were published."; }
        catch (Exception ex) { Status.Text = "Could not convert: " + ex.Message; }
        finally
        {
            operation.Dispose();
            operation = null;
            CancelButton.IsVisible = false;
            ChooseButton.IsEnabled = true;
            SetCategory(CategoryLabel.Text!.ToLowerInvariant());
        }
    }

    private async void OpenOutput(object? sender, RoutedEventArgs e)
    {
        try
        {
            Directory.CreateDirectory(destination);
            if (!await Launcher.LaunchUriAsync(new Uri(destination + Path.DirectorySeparatorChar)))
                Status.Text = "Open this folder in Files: " + destination;
        }
        catch (Exception ex) { Status.Text = ex.Message; }
    }

    private void CancelConversion(object? sender, RoutedEventArgs e) => operation?.Cancel();
    private async Task LoadOpeningShortcut()
    {
        SaveShortcutButton.IsEnabled = false;
        ShortcutFeedback.Text = "";
        try
        {
            var binding = await OpeningShortcut.ReadAsync();
            ShortcutCtrl.IsChecked = binding.Contains("<Control>") || binding.Contains("<Ctrl>") || binding.Contains("<Primary>");
            ShortcutAlt.IsChecked = binding.Contains("<Alt>");
            ShortcutShift.IsChecked = binding.Contains("<Shift>");
            ShortcutSuper.IsChecked = binding.Contains("<Super>");
            var key = System.Text.RegularExpressions.Regex.Replace(binding, "<[^>]+>", "").ToUpperInvariant();
            ShortcutKey.SelectedItem = key;
            CurrentShortcut.Text = "Current: " + DisplayShortcut(binding);
            SaveShortcutButton.IsEnabled = true;
        }
        catch (Exception ex)
        {
            CurrentShortcut.Text = "Shortcut unavailable";
            ShortcutFeedback.Text = ex.Message;
        }
    }

    private static string DisplayShortcut(string binding) => binding.Replace("<Control>", "Ctrl + ")
        .Replace("<Ctrl>", "Ctrl + ").Replace("<Primary>", "Ctrl + ").Replace("<Alt>", "Alt + ")
        .Replace("<Shift>", "Shift + ").Replace("<Super>", "Super + ").TrimEnd().TrimEnd('+').Trim();

    private async void SaveOpeningShortcut(object? sender, RoutedEventArgs e)
    {
        if (ShortcutKey.SelectedItem is not string key ||
            !(ShortcutCtrl.IsChecked == true || ShortcutAlt.IsChecked == true || ShortcutSuper.IsChecked == true))
        {
            ShortcutFeedback.Text = "Choose a key and at least Ctrl, Alt, or Super.";
            return;
        }
        var binding = (ShortcutCtrl.IsChecked == true ? "<Control>" : "") +
            (ShortcutAlt.IsChecked == true ? "<Alt>" : "") +
            (ShortcutShift.IsChecked == true ? "<Shift>" : "") +
            (ShortcutSuper.IsChecked == true ? "<Super>" : "") + (key.Length == 1 ? key.ToLowerInvariant() : key);
        SaveShortcutButton.IsEnabled = false;
        try
        {
            await OpeningShortcut.SaveAsync(binding);
            CurrentShortcut.Text = "Current: " + DisplayShortcut(binding);
            ShortcutFeedback.Text = "Saved. Your new shortcut works from anywhere.";
        }
        catch (Exception ex) { ShortcutFeedback.Text = ex.Message; }
        finally { SaveShortcutButton.IsEnabled = true; }
    }

    private void ChangeAppleColor(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string color }) return;
        var colors = color switch
        {
            "red" => new[] { "#D9FFE8E7", "#B3EDA5AB", "#CCDCA0AC" },
            "green" => new[] { "#D9EDF9E9", "#B3ADD4B6", "#CCA4C9B9" },
            "yellow" => new[] { "#D9FFF8DB", "#B3EED28D", "#CCDCC692" },
            "gray" => new[] { "#D9F3F3F3", "#B3CDCDCD", "#CCBDBDBD" },
            _ => null
        };
        if (colors is null) return;
        Resources["AppleGlass"] = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(.8, 1, RelativeUnit.Relative),
            GradientStops = new GradientStops
            {
                new(Color.Parse(colors[0]), 0),
                new(Color.Parse(colors[1]), .55),
                new(Color.Parse(colors[2]), 1)
            }
        };
        ColorButton.Flyout?.Hide();
    }

    private void MiniPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        miniPress = e;
        miniPressPoint = e.GetPosition(this);
        miniDragging = false;
    }

    private void MiniMoved(object? sender, PointerEventArgs e)
    {
        if (miniPress is null || miniDragging || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        var delta = e.GetPosition(this) - miniPressPoint;
        if (delta.X * delta.X + delta.Y * delta.Y < 25) return;
        miniDragging = true;
        BeginMoveDrag(miniPress);
        miniPress = null;
        e.Handled = true;
    }
    private void CloseApple(object? sender, RoutedEventArgs e) => Close();
    private void MoveApple(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) BeginMoveDrag(e);
    }
    private void CollapseApple(object? sender, RoutedEventArgs e)
    {
        if (operation is not null) return;
        Expanded.IsVisible = false; Collapsed.IsVisible = true; Width = 110; Height = 135;
    }
    private void ExpandApple(object? sender, RoutedEventArgs e)
    {
        if (miniDragging) return;
        ShowExpanded();
    }
    private void ShowExpanded()
    {
        miniPress = null;
        miniDragging = false;
        Width = 400; Height = 456; Collapsed.IsVisible = false; Expanded.IsVisible = true;
    }
}
