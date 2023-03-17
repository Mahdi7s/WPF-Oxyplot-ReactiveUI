using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reactive.Disposables;
using ReactiveUI;

namespace WpfChallenge
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            ViewModel = new AppViewModel();

            this.WhenActivated(disposableRegistration =>
            {
                this.OneWayBind(ViewModel, vm => vm.IsDataFileAvailable, v => v.FittingModel.IsEnabled)
                    .DisposeWith(disposableRegistration);

                this.Bind(ViewModel, vm => vm.DataFilePath, v =>  v.DataFilePath.Text).DisposeWith(disposableRegistration);
                
                this.OneWayBind(ViewModel, vm => vm.FittingModels, v=>v.FittingModel.ItemsSource).DisposeWith(disposableRegistration);
                this.Bind(ViewModel, vm => vm.FittingModel, v => v.FittingModel.SelectedItem).DisposeWith(disposableRegistration);

                this.OneWayBind(ViewModel, vm => vm.PlotModel, v=>v.PlotModel.Model).DisposeWith(disposableRegistration);

                this.BindCommand(ViewModel, vm => vm.BrowseFilesCommand, v => v.BrowseFile).DisposeWith(disposableRegistration);

                this.OneWayBind(ViewModel, vm => vm.Coefficients, v => v.Coefficients.ItemsSource).DisposeWith(disposableRegistration);
            });
        }
    }
}
