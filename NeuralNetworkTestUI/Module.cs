﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Caliburn.Micro;
using Gemini.Framework;
using Gemini.Framework.Menus;
using Gemini.Framework.Results;
using Gemini.Modules.MainMenu.Models;
using Microsoft.Win32;
using NeuralNetworkTestUI.Services;
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
        [Import] ImageOutputViewModel _imageOutputViewModel;
        [Import] OutputViewModel _outputViewModel;
        [Import] INetworkService _networkService;

        public override IEnumerable<Type> DefaultTools
        {
            get
            {
                yield return typeof(NeuralNetworkViewModel);
                yield return typeof(TrainingViewModel);
            }
        }

        public override void Initialize()
        {
            var view = MainMenu.OfType<TextMenuItem>().First(x=>x.Text.Contains("View"));
            /*
            
            view.Add(new TextMenuItem("Neuron Outputs", OpenOutputViewer));
            view.Add(new TextMenuItem("Connection Weights", OpenWeightViewer));
            view.Add(new TextMenuItem("Network View", ShowNetwork));
            view.Add(new TextMenuItem("Calculation", ShowCalculation));
            view.Add(new TextMenuItem("Statistics", ShowTestResults));
            view.Add(new TextMenuItem("Training", ShowTraining));
            view.Add(new TextMenuItem("Image Output", ShowImageOutput));
            view.Add(new TextMenuItem("Output", ShowOutput)); 

            var fileMenu = MainMenu.All.First(x => x.Name == "File");
            fileMenu.Children.Insert(0, new TextMenuItem("_New network", CreateNetwork)
                .WithGlobalShortcut(ModifierKeys.Control, Key.N));
            fileMenu.Children.Insert(1, new TextMenuItem("_Save network", SaveNetwork));
            fileMenu.Children.Insert(2, new TextMenuItem("_Save network as", SaveNetworkAs)
                .WithGlobalShortcut(ModifierKeys.Control, Key.S));
           */
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

        private IEnumerable<IResult> ShowImageOutput()
        {
            yield return Show.Tool<ImageOutputViewModel>();
        }

        private IEnumerable<IResult> ShowOutput()
        {
            yield return Show.Tool<OutputViewModel>();
        }

        private IEnumerable<IResult> CreateNetwork()
        {
            yield return Show.Window<NetworkCreationDialogViewModel>();
        }

        private IEnumerable<IResult> SaveNetwork()
        {
            if (_networkService.Active == null)
            {
                yield return new LambdaResult(delegate { Debug.WriteLine("Error saving"); });
                yield break;
            }

            if (string.IsNullOrEmpty(_networkService.CurrentProjectFilePath))
            {
                //yield return SaveNetworkAs();
                // SAVE AS
            }

            if (_networkService.Save(_networkService.CurrentProjectFilePath))
                yield return new LambdaResult(delegate { Debug.WriteLine("Save complete"); });
            else
                yield return new LambdaResult(delegate { Debug.WriteLine("Save failed"); });

            yield break;
        }

        private IEnumerable<IResult> SaveNetworkAs()
        {
            var dialog = new SaveFileDialog { DefaultExt = _networkService.DefaultExt, AddExtension = true, Filter =  string.Format("Neural Netwrok {0}| *{0}",_networkService.DefaultExt)};
            yield return Show.CommonDialog(dialog);

            if (_networkService.Save(dialog.FileName))
                yield return new LambdaResult(delegate { Debug.WriteLine("Save complete"); });
            else
                yield return new LambdaResult(delegate { Debug.WriteLine("Save failed"); });
        }
    }
}
