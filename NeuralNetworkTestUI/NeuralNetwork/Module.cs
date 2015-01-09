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
using NeuralNetworkTestUI.NeuralNetwork.ViewModels;


namespace NeuralNetworkTestUI.NeuralNetwork
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
                    new MenuItem("Create", Create),
                    new MenuItem("Train", Train)
                });

            MainMenu.All
                .First(x => x.Name == "View")
                .Add(new MenuItem("Weight Viewer", OpenWeightViewer));
            //MainMenu.All
              //  .First(x => x.Name == "View")
                //.Add(new MenuItem("NeuralNetwork", OpenWeightViewer));
        }

        private IEnumerable<IResult> Train()
        {
            yield return new LambdaResult((c) =>
            {
                var action = new System.Action(() => IoC.Get<NeuralNetworkViewModel>().Train());
                action.BeginInvoke(new AsyncCallback((res)=> IoC.Get<NeuralNetworkViewModel>().OnTrainEnd()), this);
            });
        }

        private IEnumerable<IResult> OpenWeightViewer()
        {
            yield return new LambdaResult((c) => Shell.ShowTool(IoC.Get<WeightViewerViewModel>()));
        }

        private IEnumerable<IResult> Create()
        {
            var inspectorTool = IoC.Get<IInspectorTool>();
            inspectorTool.SelectedObject = new InspectableObjectBuilder()
                .WithObjectProperties(new ConstructParameters(), pf=> true)
                .ToInspectableObject();
                //.WithCollapsibleGroup("Layers", l=>l.WithObjectProperties())
            


            yield return new LambdaResult((c) =>
            {
                var vm = IoC.Get<NeuralNetwork.ViewModels.NeuralNetworkViewModel>();
                Shell.OpenDocument(vm);
                Shell.ShowTool(inspectorTool);
            });
        }
    }
}
