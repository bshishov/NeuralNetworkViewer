namespace ShNeuralNetwork
{
    public class LinearNormalization : INormalization
    {
        public readonly double MinValue;
        public readonly double MaxValue;
        public readonly double TargetMinValue;
        public readonly double TargetMaxValue;
        public readonly double Range;
        public readonly double TargetRange;

        public LinearNormalization(double min, double max, double targetMin, double targetMax)
        {
            MinValue = min;
            MaxValue = max;
            TargetMaxValue = targetMax;
            TargetMinValue = targetMin;
            Range = MaxValue - MinValue;
            TargetRange = TargetMaxValue - TargetMinValue;
        }

        public double Normalize(double value)
        {
            return ((value - MinValue) / Range) * TargetRange + TargetMinValue;
        }

        public double Denormalize(double value)
        {
            return ((value - TargetMinValue) / TargetRange) * Range + MinValue;
        }
    }
}