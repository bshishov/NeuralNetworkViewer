using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows;
using Caliburn.Micro;
using Gemini.Framework;
using Gemini.Modules.Output;
using NeuralNetworkLibBase;
using NeuralNetworkTestUI.Messaging;
using NeuralNetworkTestUI.Views;

namespace NeuralNetworkTestUI.ViewModels
{
    [Export(typeof(NeuralNetworkViewModel))]
    class NeuralNetworkViewModel : Document, IHandle<NetworkUpdatedMessage>
    {
        public string DisplayName
        {
            get { return "Neural network"; }
        }

        private INeuralNetwork _network;
        private readonly IOutput _output;
        private IEventAggregator _events;

        public INeuralNetwork Network
        {
            get { return _network; }
            set
            {
                _network = value;
                NotifyOfPropertyChange(()=>Network);
                _events.Publish(new NetworkUpdatedMessage(_network, NetworkUpdateType.NewNetwork));
            }
        }

        [ImportingConstructor]
        public NeuralNetworkViewModel(IEventAggregator events)
        {
            _events = events;
            _events.Subscribe(this);
            _output = IoC.Get<IOutput>();
        }

        public void Train()
        {
            _output.AppendLine("Training initiated");
            var random = new Random();
            const double samples = 5000.0;
            var left = samples;
            while (left-- > 0)
            {
                if (left%100 == 0)
                {
                    _output.AppendLine(String.Format("Training: {0} / {1}", samples - left, samples));
                    _events.Publish(new NetworkUpdatedMessage(_network, NetworkUpdateType.SmallChanges));
                }
                var a = random.Next(1, 7);
                var b = random.Next(1, 7);
                var res = F(a, b);
                _network.Train( new double[] { a, b }, new double[] { res });
            }
            _output.AppendLine("Training done");
            _events.Publish(new NetworkUpdatedMessage(_network, NetworkUpdateType.SmallChanges));
        }

        
        public void Test()
        {
            var random = new Random();
            for (int i = 0; i < 100; i++)
            {
                var a = random.Next(1, 7);
                var b = random.Next(1, 7);
                var res = F(a, b);
                var result = _network.Calculate(new double[]{a, b});
                Console.WriteLine("Test{0}:\tf({1},{2}) = {3}\t(exact: {4})", i, a, b, result[0], res);
            }
        }

        static double F(double a, double b)
        {
            return a * b;
        }

        public void Handle(NetworkUpdatedMessage message)
        {
            if(message.UpdateType == NetworkUpdateType.SmallChanges)
                ((NeuralNetworkView) GetView()).Refresh();
        }
    }
}
