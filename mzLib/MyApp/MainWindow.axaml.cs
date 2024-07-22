using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using MassSpectrometry;
using Readers;
using System.Collections.Generic;
using System.Linq;

namespace MyApp;
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void OpenFileButton_Clicked(object sender, RoutedEventArgs args)
    {
        // Get top level from the current control. Alternatively, you can use Window reference instead.
        var topLevel = TopLevel.GetTopLevel(this);

        // Start async operation to open the dialog.
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Text File",
            AllowMultiple = true
        });

        Dictionary<string, List<IRetentionTimeAlignable>> filesToShow = new();

        if (files.Count >= 1)
        {
            foreach (var file in files)
            {
                var psms = SpectrumMatchTsvReader.ReadPsmTsv(file.Path.LocalPath, out _)
                    .Cast<IRetentionTimeAlignable>().ToList();
                filesToShow.Add(file.Name, psms);
            }
        }

        filesLoaded.ItemsSource = filesToShow.Keys;
    }
}
