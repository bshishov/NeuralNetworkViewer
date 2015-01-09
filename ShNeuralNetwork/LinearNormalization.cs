namespace ShNeuralNetwork
{
    public class LinearNormalization : INormalization
    {
        public readonly double MinValue;
        public readonly double MaxValue;

        public LinearNormalization(double min, double max)
        {
            MinValue = min;
            MaxValue = max;
        }

        public double Normalize(double value)
        {
            return (value - MinValue)/(MaxValue - MinValue);
        }

        public double Denormalize(double value)
        {
            return MinValue + value*(MaxValue - MinValue);
        }
    }
}