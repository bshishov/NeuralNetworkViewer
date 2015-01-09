namespace NeuralNetworkLibBase
{
    public interface IConnection
    {
        INode StartNode { get; }
        double Weight { get; }
    }
}