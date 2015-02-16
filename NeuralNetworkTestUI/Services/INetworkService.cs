using NeuralNetworkLibBase;

namespace NeuralNetworkTestUI.Services
{
    interface INetworkService
    {
        INeuralNetwork Active { get; }
        string CurrentProjectFilePath { get; }
        string DefaultExt { get; }
        bool Create(INetworkDescription description, object args);
        bool FromFile(string path);
        bool Save(string path);
    }
}
