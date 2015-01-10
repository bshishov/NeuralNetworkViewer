using NeuralNetworkLibBase;

namespace ShNeuralNetwork
{
    public class Link : IConnection
    {
        public Neuron StartNeuron;
        public INode StartNode { get { return StartNeuron; } }
        public double Weight { get; set; }

        public Link()
        {
            Weight = 1;
        }
    }
}