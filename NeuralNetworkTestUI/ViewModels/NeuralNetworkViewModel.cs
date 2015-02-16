using System.ComponentModel.Composition;
using Caliburn.Micro;
using Gemini.Framework;
using Gemini.Framework.Services;
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

        public INeuralNetwork Network
        {
            get { return _network; }
            set
            {
                _network = value;
                NotifyOfPropertyChange(()=>Network);
            }
        }

        [ImportingConstructor]
        public NeuralNetworkViewModel(IEventAggregator events)
        {
            events.Subscribe(this);
        }

        public void Handle(NetworkUpdatedMessage message)
        {
            if (message.UpdateType == NetworkUpdateType.NewNetwork)
                Network = message.Network;
            var view = GetView() as NeuralNetworkView;
            if(view == null)
                IoC.Get<IShell>().OpenDocument(this);
            else
                view.Refresh();
        }
    }
}
