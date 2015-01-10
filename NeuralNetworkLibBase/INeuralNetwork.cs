﻿using System.Collections.Generic;

namespace NeuralNetworkLibBase
{
    public interface INeuralNetwork
    {
        ILayer InputLayer { get; }
        ILayer OutputLayer { get; }

        IEnumerable<ILayer> HiddenLayers { get; }
        IEnumerable<IConnection> Connections { get; }

        void Create(ConstructionParameters parameters);
        void Train(double[] inputs, double[] outputs);
        List<double> Calculate(double[] inputs);
    }
}