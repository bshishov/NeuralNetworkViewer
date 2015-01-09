
namespace NeuralNetworkTestUI.NeuralNetwork
{
    public class LayerConstrucParameters
    {
        public int NeuronCount;
    }

    public class ConstructParameters
    {
        public int InputsCount { get; set; }
        public int OutputsCount { get; set; }
        public LayerConstrucParameters[] Layers { get; set; }

    }
}
