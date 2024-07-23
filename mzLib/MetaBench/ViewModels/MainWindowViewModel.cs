using MassSpectrometry;
using System.Collections.Generic;

namespace MetaBench.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
#pragma warning disable CA1822 // Mark members as static
    public string Greeting => "Welcome to MetaBench!";
#pragma warning restore CA1822 // Mark members as static

    private Dictionary<string, Dictionary<string, IRetentionTimeAlignable>> Results { get; set; }
}
