using Readers;
using System.Windows;
using System.Windows.Input;
using Easy.Common.Extensions;
using Proteomics.PSM;
using ToolKitUI.Util;

namespace ToolKitUI.ViewModels
{
    public class ChronologerEstimatorForResultsViewModel : ViewModelBase
    {
        private PsmFromTsvFile _psmFromTsvFile;
        private List<PsmFromTsv> _psms;

        private string _filePath;

        public PsmFromTsvFile PsmFile
        {
            get
            {
                return _psmFromTsvFile;
            }
            set
            {
                _psmFromTsvFile = value;
                Psms = _psmFromTsvFile.Results;
                OnPropertyChanged(nameof(PsmFile));
            }
        }

        public List<PsmFromTsv> Psms
        {
            get
            {
                if (_psmFromTsvFile.IsNotNullOrEmpty())
                {
                    return _psmFromTsvFile.Results;
                }

                return null;
            }
            set
            {
                _psms = value;
                OnPropertyChanged(nameof(Psms));
            }
        }

        public string FilePath
        {
            get
            {
                return _filePath;
            }
            set
            {
                _filePath = value;
                OnPropertyChanged(nameof(FilePath));
            }
        }

        public ChronologerEstimatorForResultsViewModel()
        {
            LoadFileCommand = new RelayCommand(GetFile);
            RunChronologerCommand = new RelayCommand(RunChronologer);
            WriteFileCommand = new RelayCommand(WriteFile);
        }

        public ICommand LoadFileCommand { get; }
        public ICommand RunChronologerCommand { get; }
        public ICommand WriteFileCommand { get; }


        private async void GetFile()
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
                FilePath = dialog.FileName;
                PsmFile = new PsmFromTsvFile(FilePath);
            }

            // load the results async
            Task loadResults = new Task(() =>
            {
                PsmFile.LoadResults();
            });

            loadResults.Start();

            await loadResults;
        }

        private void RunChronologer()
        {

        }

        private void WriteFile()
        {
        }
    }
}
