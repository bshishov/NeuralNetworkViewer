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

        public void Handle(NetworkUpdatedMessage message)
        {
            if(message.UpdateType == NetworkUpdateType.SmallChanges)
                ((NeuralNetworkView) GetView()).Refresh();
        }
    }
}
