using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using Caliburn.Micro;
using Gemini.Framework;
using Gemini.Framework.Services;
using NeuralNetworkLibBase;
using NeuralNetworkTestUI.Messaging;
using NeuralNetworkTestUI.Views;

namespace NeuralNetworkTestUI.ViewModels
{
    [Export(typeof(OutputViewerViewModel))]
    class OutputViewerViewModel : Tool, IHandle<NetworkUpdatedMessage>
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
                NotifyOfPropertyChange(()=>SelectedNetwork);
                UpdateCollections();
            }
        }

        public OutputViewerViewModel()
        {
            DisplayName = "Ouptuts viewer";
            Collections = new ObservableCollection<NodeNodeCollection>();
        }

        [ImportingConstructor]
        public OutputViewerViewModel(IEventAggregator events) : this()
        {
            events.Subscribe(this);
        }

        public void UpdateCollections()
        {
            if(_network == null)
                return;
            Collections.Clear();
            var inputCollection = new NodeNodeCollection() { Name = "Input layer" };
            var outputCollection = new NodeNodeCollection() { Name = "Output layer" };

            foreach (var neuron in _network.InputLayer.Nodes)
            {
                inputCollection.Members.Add(new NodeNode(neuron) { Name = "Input Neuron" });
            }

            foreach (var neuron in _network.OutputLayer.Nodes)
            {
                outputCollection.Members.Add(new NodeNode(neuron) { Name = "Output Neuron" });
            }

            Collections.Add(inputCollection);
            foreach (var layer in _network.HiddenLayers)
            {
                var collection = new NodeNodeCollection()
                {
                    Name = "Hidden layer"
                };
                foreach (var neuron in layer.Nodes)
                {
                    collection.Members.Add(new NodeNode(neuron) { Name = "Neuron" });
                }
                Collections.Add(collection);
            }
            Collections.Add(outputCollection);
        }

        public ObservableCollection<NodeNodeCollection> Collections { get; set; }

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

    public class NodeNodeCollection : NodeCollection<NodeNode> { }
    public class ConnectionNodeCollection : NodeCollection<ConnectionNode> { }

    public class NodeCollection<T>
        where T : ITreeviewNode
    {
        public ObservableCollection<T> Members { get; set; }

        public NodeCollection()
        {
            this.Members = new ObservableCollection<T>();
            this.Members.CollectionChanged += (sender,args) => Update();
        }

        public string Name { get; set; }

        public void Update()
        {
            foreach (var member in Members)
            {
                member.UpdateValue();
            }
        }
    }

    public interface ITreeviewNode
    {
        void UpdateValue();
    }

    public class NodeNode : PropertyChangedBase, ITreeviewNode
    {
        public string Name { get; set; }
        public double Value { get { return _node.Output; } }
        private readonly INode _node;

        public NodeNode(INode node)
        {
            _node = node;
        }

        public void UpdateValue()
        {
            NotifyOfPropertyChange(()=>Value);
        }
    }

    public class ConnectionNode : PropertyChangedBase, ITreeviewNode
    {
        public string Name { get; set; }
        public double Value { get { return _node.Weight; } }
        private readonly IConnection _node;

        public ConnectionNode(IConnection node)
        {
            _node = node;
        }

        public void UpdateValue()
        {
            NotifyOfPropertyChange(() => Value);
        }
    }
}
