using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassSpectrometry;

public class AlignedPsm : IRetentionTimeAlignable
{
    public string FileName { get; set; }
    public float RetentionTime { get; set; }
    public string Identifier { get; }
    public float TimeShift { get; set; }
    public float ChronologerHI { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string BaseSequence { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string FullSequence { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
}
