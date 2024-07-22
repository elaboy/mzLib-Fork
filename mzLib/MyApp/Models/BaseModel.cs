using System.Collections.Generic;
using System.Collections.ObjectModel;
using MassSpectrometry;
using ScottPlot.Avalonia;

namespace MyApp.Models;
internal class BaseModel
{
    public Dictionary<string, IRetentionTimeAlignable>? PsmFiles { get; set; }
    public List<string>? Runners { get; set; }
    public AvaPlot? ResultsOne { get; set; }
    public AvaPlot? ResultsTwo { get; set; }

}
