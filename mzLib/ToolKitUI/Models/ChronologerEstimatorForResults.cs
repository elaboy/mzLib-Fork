using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Readers;

namespace ToolKitUI.Models
{
    public class ChronologerEstimatorForResults
    {
        private readonly PsmFromTsvFile psmFromTsvFile;
        public string filePath { get; set; }
        public ChronologerEstimatorForResults(string resultsPath)
        {
            filePath = resultsPath;
            psmFromTsvFile = new PsmFromTsvFile(filePath);
            Task loadResults = new Task(() =>
            {
                psmFromTsvFile.LoadResults();
            });
            loadResults.Start();
        }
    }
}
