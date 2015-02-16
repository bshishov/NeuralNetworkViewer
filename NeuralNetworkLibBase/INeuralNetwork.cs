using System;
using System.Collections.Generic;

namespace NeuralNetworkLibBase
{
    public interface INetworkDescription
    {
        string Name { get; }
        string Description { get; }
        string Author { get; }
        Type ArgsType { get; }
        INeuralNetwork Create(object args);
    }

    public interface INeuralNetwork
    {
        ILayer InputLayer { get; }
        ILayer OutputLayer { get; }
        

        IEnumerable<ILayer> HiddenLayers { get; }
        IEnumerable<IConnection> Connections { get; }
        
        double Train(double[] inputs, double[] outputs);
        List<double> Calculate(double[] inputs);
    }
}