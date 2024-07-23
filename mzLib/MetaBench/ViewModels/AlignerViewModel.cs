using System.Collections.Generic;
using MassSpectrometry;
using ReactiveUI;

namespace MetaBench.ViewModels;
internal class AlignerViewModel : MainWindowViewModel
{
    public AlignerViewModel()
    {
        this.WhenAnyValue(x => x.Results);
    }

    private Dictionary<string, Dictionary<string, IRetentionTimeAlignable>> Results;
}
