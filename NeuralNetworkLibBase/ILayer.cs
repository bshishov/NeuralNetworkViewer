using System.Collections.Generic;

namespace NeuralNetworkLibBase
{
    public interface ILayer
    {
        IEnumerable<INode> Nodes { get; }
    }
}