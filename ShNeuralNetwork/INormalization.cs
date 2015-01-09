namespace ShNeuralNetwork
{
    public interface INormalization
    {
        double Normalize(double value);
        double Denormalize(double value);
    }
}