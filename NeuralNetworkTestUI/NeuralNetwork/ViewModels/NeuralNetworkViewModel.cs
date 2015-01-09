using System;
using System.ComponentModel.Composition;
using System.Net.Mime;
using System.Windows;
using Caliburn.Micro;
using Gemini.Framework;
using Gemini.Modules.Output;
using ShNeuralNetwork;

namespace NeuralNetworkTestUI.NeuralNetwork.ViewModels
{
    [Export(typeof(NeuralNetworkViewModel))]
    class NeuralNetworkViewModel : Document
    {
        public string DisplayName
        {
            get { return "Neural network"; }
        }

        private ShNeuralNetwork.NeuralNetwork _network;
        private readonly IOutput _output;

        public ShNeuralNetwork.NeuralNetwork Network
        {
            get { return _network; }
            set
            {
                _network = value;
                NotifyOfPropertyChange(()=>Network);
            }
        }
        
        public NeuralNetworkViewModel()
        {
            _output = IoC.Get<IOutput>();
            _network = new ShNeuralNetwork.NeuralNetwork(2, new int[] { 5, 10, 5, 1 }, SquashingFunctions.Sigmoid,
                new LinearNormalization(0, 10),
                new LinearNormalization(-10, 50));
            IoC.Get<WeightViewerViewModel>().SelectedNetwork = Network;
        }

        public NeuralNetworkViewModel(ConstructParameters parameters)
        {
            _output = IoC.Get<IOutput>();
            IoC.Get<WeightViewerViewModel>().SelectedNetwork = Network;
        }

        public void Train()
        {
            _output.AppendLine("Training initiated");
            var random = new Random();
            const double samples = 10000.0;
            var left = samples;
            while (left-- > 0)
            {
                if (left % 100 == 0) _output.Append(".");
                var a = random.Next(1, 7);
                var b = random.Next(1, 7);
                var res = F(a, b);
                _network.TrainSingle(
                    new double[] { res },
                    new double[] { a, b },
                    left / samples + 0.1
                    );
            }

            _output.Append("!\n");
            _output.AppendLine("Training done");
        }

        public void OnTrainEnd()
        {
            Application.Current.Dispatcher.Invoke(() =>
                IoC.Get<WeightViewerViewModel>().UpdateCollections());
        }

        public void Test()
        {
            var random = new Random();
            for (int i = 0; i < 100; i++)
            {
                var a = random.Next(1, 7);
                var b = random.Next(1, 7);
                var res = F(a, b);
                var result = _network.Calculate(a, b);
                Console.WriteLine("Test{0}:\tf({1},{2}) = {3}\t(exact: {4})", i, a, b, result[0], res);
            }
        }

        static double F(double a, double b)
        {
            return Math.Sin(a) - Math.Cos(b);
        }
    }
}
