using System.Collections.Generic;
using NeuralNetworkLibBase;

namespace ShNeuralNetwork
{
    public class Layer : ILayer
    {
        public readonly List<Neuron> Neurons = new List<Neuron>();

        public Layer()
        {
            
        }

        public IEnumerable<INode> Nodes { get { return Neurons; } }
    }
}