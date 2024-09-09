using Easy.Common.Extensions;
using Proteomics.PSM;
using Proteomics.RetentionTimePrediction.Chronologer;
using Readers;
using RTLib;
using System.Windows.Input;
using ToolKitUI.Util;

namespace ToolKitUI.ViewModels
{
    public class ChronologerEstimatorForResultsViewModel : ViewModelBase
    {
        private PsmFromTsvFile _psmFromTsvFile;
        private List<PsmFromTsv> _psms;
        private string _filePath;
        private List<LightPsm> _lightPsms;
        private List<PsmFromTsv> _filteredPsms;
        private bool _nonAmbiguousPsms = false;

        public List<PsmFromTsv> FilteredPsms
        {
            get
            {
                return _filteredPsms;
            }
            set
            {
                _filteredPsms = value;
                OnPropertyChanged(nameof(FilteredPsms));
            }
        }

        public bool NonAmbiguousPsms
        {
            get
            {
                return _nonAmbiguousPsms;
            }
            set
            {
                _nonAmbiguousPsms = value;
                OnPropertyChanged(nameof(NonAmbiguousPsms));
            }
        }
        public List<LightPsm> LightPsms
        {
            get
            {
                return _lightPsms;
            }
            set
            {
                _lightPsms = value;
                OnPropertyChanged(nameof(LightPsms));
            }
        }
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
            ShowNonAmbiguousPsms = new RelayCommand(ToggleNonAmbiguousPsms);
        }

        public ICommand LoadFileCommand { get; }
        public ICommand RunChronologerCommand { get; }
        public ICommand ShowNonAmbiguousPsms { get; }
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
            float[] chronologerPredictions =
                ChronologerEstimator.PredictRetentionTime(
                    FilteredPsms.Select(x => x.BaseSequence).ToArray(),
                    FilteredPsms.Select(x => x.FullSequence).ToArray());

            LightPsm[] psms = new LightPsm[FilteredPsms.Count];

            Parallel.For(0, psms.Length, idx =>
            {
                psms[idx] = new LightPsm()
                {
                    FileName = FilteredPsms[idx].FileName,
                    BaseSequence = FilteredPsms[idx].BaseSequence,
                    FullSequence = FilteredPsms[idx].FullSequence,
                    RetentionTime = (float)FilteredPsms[idx].RetentionTime.Value,
                    ChronologerHI = chronologerPredictions[idx]
                };
            });

            LightPsms = psms.ToList();
        }

        private void ToggleNonAmbiguousPsms()
        {
            if (!NonAmbiguousPsms)
            {
                FilteredPsms = Psms.Where(x => x.AmbiguityLevel == "1").ToList();
                NonAmbiguousPsms = true;
            }
            else
            {
                FilteredPsms = Psms;
                NonAmbiguousPsms = false;
            }
        }

        private void WriteFile()
        {
        }
    }
}
