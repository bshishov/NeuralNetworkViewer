using System;

namespace ShNeuralNetwork
{
    public static class SquashingFunctions
    {
        public static SquashingFunction Sigmoid = new SquashingFunction(
            (net) => 1/(1 + Math.Pow(Math.E, - net)),
            (net) => (1 / (1 + Math.Pow(Math.E, -net))) * (1 - 1 / (1 + Math.Pow(Math.E, -net)))
            );
        public static SquashingFunction BipolarSigmoid = new SquashingFunction(
            (x) => (2 / (1 + Math.Pow(Math.E, -x)) - 1),
            (x) => 0.5 * (1 + (2 / (1 + Math.Pow(Math.E, -x)) - 1)) * (1 - (2 / (1 + Math.Pow(Math.E, -x)) - 1))
            );
        //public static SquashingFunction Discrete = (net) => net > 0 ? 1 : 0;
        //public static SquashingFunction BipolarDiscrete = (net) => net > 0 ? 1 : -1;
        public static SquashingFunction Linear = new SquashingFunction(
            (x) => x <= 0 ? 0 : x >= 1 ? 1 : x ,
            (x) => x <= 0 ? 0 : x >= 1 ? 0 : 1
            );
    }
}