namespace ShNeuralNetwork
{
    public class Input : Neuron
    {
        public Input() : base()
        {
        }

        public void Set(double input)
        {
            this._output = input;
        }
    }
}