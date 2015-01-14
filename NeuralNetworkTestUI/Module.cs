using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Caliburn.Micro;
using Gemini.Framework;
using Gemini.Framework.Results;
using Gemini.Modules.Inspector;
using Gemini.Modules.MainMenu.Models;
using Gemini.Modules.Output.ViewModels;
using NeuralNetworkTestUI.ViewModels;
using NeuralNetworkTestUI.Views;


namespace NeuralNetworkTestUI
{
    [Export(typeof(IModule))]
    class Module : ModuleBase
    {
        [Import]
        private NeuralNetworkViewModel _neuralNetworkViewModel;
        
        public override void Initialize()
        {           
            MainMenu.Add(
                new MenuItem("Neural Network")
                {
                    new MenuItem("Train", Train)
                });

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

        private IEnumerable<IResult> Train()
        {
            yield return new LambdaResult((c) =>
            {
                var action = new System.Action(() => ((NeuralNetworkViewModel)Shell.ActiveItem).Train());
                action.BeginInvoke(new AsyncCallback((res)=> {}), this);
            });
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
            yield return Show.Tool<StatisticsViewModel>();
        }

        private IEnumerable<IResult> ShowTraining()
        {
            yield return Show.Tool<TrainingViewModel>();
        }

        private IEnumerable<IResult> CreateNetwork()
        {
            //yield return Show.Window<NetworkCreationDialogViewModel>();
            yield return Show.Window<NetworkCreationDialogViewModel>();
        }
    }
}
