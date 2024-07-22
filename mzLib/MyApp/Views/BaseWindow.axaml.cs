using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using MassSpectrometry;
using Readers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyApp.Views
{
    public partial class BaseWindow : Window
    {
        public BaseWindow()
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

            if (files.Count >= 1)
            {
                Dictionary<string, List<IRetentionTimeAlignable>> filesLoaded = new();
                foreach (var file in files)
                {
                    var psms = SpectrumMatchTsvReader.ReadPsmTsv(file.Name, out _)
                        .Cast<IRetentionTimeAlignable>().ToList();
                    filesLoaded.Add(file.Name, psms);
                }
            }


        }
    }
}
