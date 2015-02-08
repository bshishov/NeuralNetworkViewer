using System;
using System.Collections.Generic;

namespace NeuralNetworkLibBase
{
    public interface INeuralNetwork
    {
        string Name { get; }
        string Description { get; }
        string Author { get; }

        ILayer InputLayer { get; }
        ILayer OutputLayer { get; }
        Type ArgsType { get; }

        IEnumerable<ILayer> HiddenLayers { get; }
        IEnumerable<IConnection> Connections { get; }

        void Create(object args);
        void Train(double[] inputs, double[] outputs);
        List<double> Calculate(double[] inputs);
    }
}