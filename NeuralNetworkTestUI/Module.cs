using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Caliburn.Micro;
using Gemini.Framework;
using Gemini.Framework.Results;
using Gemini.Modules.MainMenu.Models;
using NeuralNetworkTestUI.ViewModels;


namespace NeuralNetworkTestUI
{
    [Export(typeof(IModule))]
    class Module : ModuleBase
    {
        [Import] NeuralNetworkViewModel _neuralNetworkViewModel;
        [Import] CalculationViewModel _calculationViewModel;
        [Import] StatisticsViewModel _statisticsViewModel;
        [Import] GenerateDataViewModel _generateDataViewModel;
        [Import] OutputViewerViewModel _outputViewerViewModel;
        [Import] WeightViewerViewModel _weightViewerViewModel;
        [Import] NetworkCreationDialogViewModel _networkCreationDialogViewModel;

        public override void Initialize()
        {   
            var view = MainMenu.All.First(x => x.Name == "View");
            view.Add(new MenuItem("Output Viewer", OpenOutputViewer));
            view.Add(new MenuItem("Weight Viewer", OpenWeightViewer));
            view.Add(new MenuItem("Neural Network", ShowNetwork));
            view.Add(new MenuItem("Calculation", ShowCalculation));
            view.Add(new MenuItem("Statistics Viewer", ShowTestResults));
            view.Add(new MenuItem("Training", ShowTraining));
            MainMenu.All
                .First(x => x.Name == "File")
                .Add(new MenuItem("Create network", CreateNetwork));
        }

        private IEnumerable<IResult> OpenOutputViewer()
        {
            yield return Show.Tool<OutputViewerViewModel>();
        }

        private IEnumerable<IResult> OpenWeightViewer()
        {
            yield return Show.Tool<WeightViewerViewModel>();
        }

        private IEnumerable<IResult> ShowNetwork()
        {
            yield return Show.Document<NeuralNetworkViewModel>();
        }

        private IEnumerable<IResult> ShowCalculation()
        {
            yield return Show.Tool<CalculationViewModel>();
        }

        private IEnumerable<IResult> ShowTestResults()
        {
            yield return Show.Document<StatisticsViewModel>();
        }

        private IEnumerable<IResult> ShowTraining()
        {
            yield return Show.Tool<TrainingViewModel>();
        }

        private IEnumerable<IResult> CreateNetwork()
        {
            yield return Show.Window<NetworkCreationDialogViewModel>();
        }
    }
}
