using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Nashi.Core;

namespace Molla.Linux;

public sealed partial class MainWindow : Window
{
    private readonly List<string> files = [];
    private AppleWindow? apple;

    private void ShowApple(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (apple is null)
        {
            apple = new AppleWindow();
            apple.Closed += (_, _) => apple = null;
            apple.Show();
        }
        apple.Activate();
    }

    public MainWindow()
    {
        InitializeComponent();
        RefreshSelection();
        Opened += (_, _) => Console.WriteLine("Molla window opened.");
    }

    private async void ChooseFiles(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var selected = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Choose files for Molla",
            AllowMultiple = true
        });
        foreach (var file in selected)
        {
            var path = file.TryGetLocalPath();
            if (path is not null && !files.Contains(path, StringComparer.Ordinal))
            {
                files.Add(path);
            }
        }
        RefreshSelection();
    }

    private void ClearFiles(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        files.Clear();
        RefreshSelection();
    }

    private void ShowLibrary(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Status.Text = "Library is part of the Molla desktop port.";
    }

    private void RefreshSelection()
    {
        FileList.ItemsSource = files.ToArray();
        SelectionTitle.Text = files.Count switch
        {
            0 => "Drop something good.",
            1 => Path.GetFileName(files[0]),
            _ => $"{files.Count} files, ready to go."
        };
        SelectionDescription.Text = files.Count == 1
            ? Path.GetDirectoryName(files[0]) ?? "Selected file"
            : "Images, videos, documents, and all the little things in between.";

        var formats = FeatureCatalog.Formats(files);
        FormatChoices.Children.Clear();
        foreach (var format in formats)
        {
            FormatChoices.Children.Add(new Button
            {
                Content = format.ToUpperInvariant(),
                Margin = new Avalonia.Thickness(0, 0, 7, 7),
                Padding = new Avalonia.Thickness(12, 8),
                CornerRadius = new Avalonia.CornerRadius(10),
                Background = Avalonia.Media.Brushes.White,
                Foreground = Avalonia.Media.Brushes.DarkGreen,
                IsEnabled = false
            });
        }
        FormatHeading.IsVisible = formats.Length > 0;
    }
}
