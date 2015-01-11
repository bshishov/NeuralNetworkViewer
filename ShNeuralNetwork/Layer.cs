using System.Collections.Generic;
using NeuralNetworkLibBase;

namespace ShNeuralNetwork
{
    public class Layer<T> : ILayer
        where T : Neuron
    {
        public IEnumerable<INode> Nodes { get { return Neurons; } }
        public readonly List<T> Neurons = new List<T>();

        public Layer()
        {
            
        }

        public Layer(List<T> neurons)
        {
            Neurons = neurons;
        }

    }
}