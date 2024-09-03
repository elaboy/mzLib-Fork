using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CsvHelper;
using Proteomics.RetentionTimePrediction.Chronologer;
using Readers;
using RTLib;
using Path = System.IO.Path;

namespace RtLibGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private PsmFromTsvFile file { get; set; }

        //RtLib rtLib = new RtLib();
        //List<Task> tasks = new List<Task>();
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void GetFile(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = "Document"; // Default file name
            dialog.DefaultExt = ".psmtsv"; // Default file extension
            dialog.Filter = "TSV File (.psmtsv)|*.psmtsv"; // Filter files by extension

            // Show open file dialog box
            bool? result = dialog.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                string filename = dialog.FileName;
                file = new PsmFromTsvFile(filename);
            }

            // load the results async
            Task loadResults = new Task(() =>
            {
                file.LoadResults();
            });

            loadResults.Start();

            await loadResults;
        }

        private void WriteFile(object sender, RoutedEventArgs e)
        {
            // Configure open folder dialog box
            Microsoft.Win32.OpenFolderDialog dialog = new();

            dialog.Multiselect = false;
            dialog.Title = "Select a folder";

            // Show open folder dialog box
            bool? result = dialog.ShowDialog();

            // Get the selected folder
            string fullPathToFolder = dialog.FolderName;
            string folderNameOnly = dialog.SafeFolderName;

            // predict psms HI and spit out a tsv with LighPsms

            // lightPsm array with size of the file results
            LightPsm[] lightPsms = new LightPsm[file.Results.Count];

            // predict all sequences 
            float[] chronologerPredictions = ChronologerEstimator.PredictRetentionTime(
                file.Results.Select(x => x.BaseSequence).ToArray(),
                file.Results.Select(x => x.FullSequence).ToArray(), 
                true);

            Parallel.For(0, chronologerPredictions.Length, i =>
            {
                lightPsms[i] = new LightPsm()
                {
                    BaseSequence = file.Results[i].BaseSequence,
                    ChronologerHI = chronologerPredictions[i],
                    FileName = file.Results[i].FileName,
                    FullSequence = file.Results[i].FullSequence,
                    RetentionTime = (float)file.Results[i].RetentionTime.Value,
                };
            });

            // save as tsv using CSVHelper

            using(var writer = new StreamWriter(Path.Join(fullPathToFolder,
                      file.Results.First().FileName+"WithChronologerHI.csv")))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteHeader<LightPsmMap>();
                csv.NextRecord();
                foreach (var record in lightPsms)
                {
                    csv.WriteRecord(record);
                    csv.NextRecord();
                }
            }
        }
    }
}