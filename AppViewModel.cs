using DynamicData;
using DynamicData.Binding;
using MathNet.Numerics;
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfChallenge
{
    public class AppViewModel : ReactiveObject
    {
        #region Bindings

        private string _dataFilePath = string.Empty;
        public string DataFilePath
        {
            get => _dataFilePath;
            set => this.RaiseAndSetIfChanged(ref _dataFilePath, value);
        }

        private FittingModel _fittingModel = FittingModel.None;
        public FittingModel FittingModel
        {
            get => _fittingModel;
            set => this.RaiseAndSetIfChanged(ref _fittingModel, value);
        }

        public List<FittingModel> FittingModels => Enum.GetValues(typeof(FittingModel)).Cast<FittingModel>().ToList();

        private ObservableAsPropertyHelper<PlotModel> _plotModel;
        public PlotModel PlotModel => _plotModel.Value;

        public ReactiveCommand<Unit, Unit> BrowseFilesCommand { get; }

        private readonly ObservableAsPropertyHelper<bool> _isDataFileAvailable;
        public bool IsDataFileAvailable => _isDataFileAvailable.Value;

        private SourceList<CoefficientModel> _coefficientsList = new SourceList<CoefficientModel>();
        private readonly IObservableCollection<CoefficientModel> _coefficients = new ObservableCollectionExtended<CoefficientModel>();
        public IObservableCollection<CoefficientModel> Coefficients => _coefficients;

        #endregion

        public AppViewModel()
        {
            _isDataFileAvailable = this.WhenAnyValue(x => x.DataFilePath)
                .Throttle(TimeSpan.FromMilliseconds(850))
                .Select(path => path.Trim())
                .DistinctUntilChanged()
                .Select(ValidateDataFile)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, x => x.IsDataFileAvailable);

            BrowseFilesCommand = ReactiveCommand.Create(() =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Multiselect = true;
                openFileDialog.Filter = "Text Files (*.txt)|*.txt|All files (*.*)|*.*";

                if (openFileDialog.ShowDialog() == true)
                {
                    DataFilePath = string.Join(';', openFileDialog.FileNames);
                }
            });

            _plotModel = this.WhenAnyValue(x => x.FittingModel, x => x.DataFilePath, (fm, p) => (fm, p))
                .Throttle(TimeSpan.FromSeconds(1))
                .Where(fmp => fmp.fm != FittingModel.None)
                .DistinctUntilChanged()
                .Select(fmp => GeneratePlotModel(fmp.fm))
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, x => x.PlotModel);

            _coefficientsList.Connect().ObserveOn(RxApp.MainThreadScheduler).Bind(_coefficients).Subscribe();
        }

        #region Privates

        private PlotModel GeneratePlotModel(FittingModel fm)
        {
            _coefficientsList.Clear();
            var plotModel = new PlotModel { Title = $"Fitting Model : {fm}", Background = OxyColors.WhiteSmoke };
            foreach (var dataPoints in allDataPoints)
            {
                var xArr = dataPoints.Select(_ => _.X).ToArray();
                var yArr = dataPoints.Select(_ => _.Y).ToArray();
                var abCoefficients = Fit.Line(xArr, yArr);
                var lseries = new LineSeries();
                lseries.Points.AddRange(dataPoints);
                lseries.Color = GetRandColor();
                plotModel.Series.Add(lseries);

                _coefficientsList.Add(new CoefficientModel(lseries.Color) { A = abCoefficients.A, B = abCoefficients.B });

                // for the fitted-curve
                var fseries = new LineSeries();
                fseries.Color = OxyColor.FromArgb((byte)(lseries.Color.A / 2), lseries.Color.R, lseries.Color.G, lseries.Color.B);
                var fitFunc = fm == FittingModel.Linear ? Fit.LineFunc(xArr, yArr) : fm == FittingModel.Power ? Fit.PowerFunc(xArr, yArr)
                    : Fit.ExponentialFunc(xArr, yArr);

                fseries.Points.AddRange(xArr.Select(x => new DataPoint(x, fitFunc(x))));
                plotModel.Series.Add(fseries);
            }

            return plotModel;
        }

        private List<DataPoint[]> allDataPoints;
        private bool ValidateDataFile(string dataFilePaths)
        {
            allDataPoints = new List<DataPoint[]>();
            foreach (var dataFilePath in dataFilePaths.Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                if (!string.IsNullOrWhiteSpace(dataFilePath) && File.Exists(dataFilePath))
                {
                    try
                    {
                        var data = File.ReadAllLines(dataFilePath);
                        allDataPoints.Add(data.Select(x =>
                        {
                            var xy = x.Split(",", StringSplitOptions.RemoveEmptyEntries);

                            return new DataPoint(double.Parse(xy[0]), double.Parse(xy[1]));
                        }).ToArray());
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            return allDataPoints.Count > 0;
        }

        private OxyColor GetRandColor() {
            var rand = new Random();
            return OxyColor.FromArgb(byte.MaxValue, (byte)rand.Next(256), (byte)rand.Next(256),  (byte)rand.Next(256));
        }

        #endregion
    }
}
