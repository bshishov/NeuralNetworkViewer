using NeuralNetworkLibBase;

namespace NeuralNetworkTestUI.Messaging
{ 
    enum NetworkUpdateType
    {
        NewNetwork,
        SmallChanges
    }

    class NetworkUpdatedMessage
    {
        public readonly INeuralNetwork Network;
        public readonly NetworkUpdateType UpdateType;

       

        public NetworkUpdatedMessage(INeuralNetwork network, NetworkUpdateType type)
        {
            Network = network;
            UpdateType = type;
        }
    }
}
