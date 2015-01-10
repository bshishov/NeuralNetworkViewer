using System.Collections.Generic;

namespace NeuralNetworkLibBase
{
    public class ConstructionParameters
    {
        public int Inputs { get; set; }
        public List<int> HiddenLayers { get; set; }
        public int Outputs { get; set; }
    }
}
