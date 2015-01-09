using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Gemini.Framework;
using Gemini.Framework.Services;

namespace NeuralNetworkTestUI.NeuralNetwork.ViewModels
{
    [Export(typeof(WeightViewerViewModel))]
    class WeightViewerViewModel : Tool
    {
        public override PaneLocation PreferredLocation
        {
            get { return PaneLocation.Right; }
        }

        private ShNeuralNetwork.NeuralNetwork _network;
        public ShNeuralNetwork.NeuralNetwork SelectedNetwork
        {
            get { return _network; }
            set
            {
                _network = value;
                NotifyOfPropertyChange(()=>SelectedNetwork);
                UpdateCollections();
            }
        }

        public WeightViewerViewModel()
        {
            DisplayName = "Weight viewer";
            Collections = new ObservableCollection<NodeCollection>();
        }

        public void UpdateCollections()
        {
            Collections.Clear();
            foreach (var layer in _network.Layers)
            {
                var collection = new NodeCollection()
                {
                    Name = "Layer"
                };
                foreach (var neuron in layer.Neurons)
                {
                    collection.Members.Add(new Node()
                    {
                        Name = "Neuron",
                        Weight = neuron.Output
                    });
                }
                Collections.Add(collection);
            }
        }

        public ObservableCollection<NodeCollection> Collections { get; set; }

    }
        
    public class NodeCollection
    {
        public ObservableCollection<Node> Members { get; set; }

        public NodeCollection()
        {
            this.Members = new ObservableCollection<Node>();
        }

        public string Name { get; set; }
    }

    public class Node
    {
        public string Name { get; set; }
        public double Weight { get; set; }
    }
}
