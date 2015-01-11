using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Caliburn.Micro;
using Gemini.Framework;
using Gemini.Framework.Services;
using NeuralNetworkLibBase;
using NeuralNetworkTestUI.Messaging;

namespace NeuralNetworkTestUI.ViewModels
{
    [Export(typeof(WeightViewerViewModel))]
    class WeightViewerViewModel : Tool, IHandle<NetworkUpdatedMessage>
    {
        public override PaneLocation PreferredLocation
        {
            get { return PaneLocation.Right; }
        }

        private INeuralNetwork _network;
        public INeuralNetwork SelectedNetwork
        {
            get { return _network; }
            set
            {
                _network = value;
                NotifyOfPropertyChange(() => SelectedNetwork);
                UpdateCollections();
            }
        }

        public WeightViewerViewModel()
        {
            DisplayName = "Weight viewer";
            Collections = new ObservableCollection<ConnectionNodeCollection>();
        }

        [ImportingConstructor]
        public WeightViewerViewModel(IEventAggregator events) : this()
        {
            events.Subscribe(this);
        }

        public void UpdateCollections()
        {
            if (_network == null)
                return;
            Collections.Clear();
            

            
            foreach (var layer in _network.HiddenLayers)
            {
                var collection = new ConnectionNodeCollection()
                {
                    Name = "Connections"
                };
                foreach (var neuron in layer.Nodes)
                {
                    foreach (var connection in neuron.Incoming)
                    {
                        collection.Members.Add(new ConnectionNode(connection)
                        {
                            Name = "Connection",
                        });
                    }
                }
                Collections.Add(collection);
            }
            
            
            foreach (var neuron in _network.OutputLayer.Nodes)
            {
                var collection = new ConnectionNodeCollection()
                {
                    Name = "Connections"
                };
                foreach (var connection in neuron.Incoming)
                {
                    collection.Members.Add(new ConnectionNode(connection)
                    {
                        Name = "Connection",                        
                    });    
                }
                Collections.Add(collection); 
            }
        }

        public ObservableCollection<ConnectionNodeCollection> Collections { get; set; }

        public void Handle(NetworkUpdatedMessage message)
        {
            if (message.UpdateType == NetworkUpdateType.NewNetwork)
                SelectedNetwork = message.Network;
            else
            {
                NotifyOfPropertyChange(() => Collections);
                foreach (var nodeNodeCollection in Collections)
                {
                    nodeNodeCollection.Update();
                }
            }
        }
    }
}
