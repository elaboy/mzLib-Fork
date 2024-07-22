using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using MassSpectrometry;
using MyApp.Models;
using ScottPlot.Avalonia;

namespace MyApp.ViewModels;
internal class BaseWindowViewModel
{
    public ObservableCollection<Dictionary<string, List<IRetentionTimeAlignable>>> FilesLoaded = new();
    public List<string> Files = new();
    private void AddFileToPsmsList()
    {
         
    }

}

