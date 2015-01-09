using System.Collections.Generic;

namespace NeuralNetworkLibBase
{
    public interface INode
    {
        double Output { get; }
        IEnumerable<IConnection> Incoming { get; }
    }
}