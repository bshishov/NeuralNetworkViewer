using System;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace ShNeuralNetwork
{
    public class SquashingFunction
    {
        public readonly MathFunction Main;
        public readonly MathFunction Derivative;

        public SquashingFunction(MathFunction main, MathFunction derivative)
        {
            Main = main;
            Derivative = derivative;
        }

        public static SquashingFunction FromType(SquashingFunctions type)
        {
            switch (type)
            {
                case SquashingFunctions.Sigmoid:
                    return new SquashingFunction(
                        (net) => 1/(1 + Math.Pow(Math.E, - net)),
                        (net) => (1/(1 + Math.Pow(Math.E, -net)))*(1 - 1/(1 + Math.Pow(Math.E, -net)))
                        );
                case SquashingFunctions.SigmoidCentered:
                    return new SquashingFunction(
                        (net) => 1 / (1 + Math.Pow(Math.E, -net)) - 0.5,
                        (net) => (1 / (1 + Math.Pow(Math.E, -net)) - 0.5) * (1 - 1 / (1 + Math.Pow(Math.E, -net)) - 0.5)
                        );
                case SquashingFunctions.BipolarSigmoid:
                    return new SquashingFunction(
                        (x) => (2/(1 + Math.Pow(Math.E, -x)) - 1),
                        (x) => 0.5*(1 + (2/(1 + Math.Pow(Math.E, -x)) - 1))*(1 - (2/(1 + Math.Pow(Math.E, -x)) - 1))
                        );
                case SquashingFunctions.Linear:
                    return new SquashingFunction(
                        (x) => x <= -1 ? -1 : x >= 1 ? 1 : (x + 1) / 2,
                        (x) => x <= -1 ? 0 : x >= 1 ? 0 : 1
                        );
                case SquashingFunctions.BipolarLinear:
                    return new SquashingFunction(
                        (x) => x <= -1 ? -1 : x >= 1 ? 1 : x,
                        (x) => x <= -1 ? 0 : x >= 1 ? 0 : 1
                        );
                case SquashingFunctions.BipolarArcusTangent:
                    return new SquashingFunction(
                        (x) => 2 * Math.Atan(x) / Math.PI,
                        (x) => 2 * Math.Pow(Math.Sin(2 * Math.Atan(x)), 2) / Math.PI
                        );
                case SquashingFunctions.Cauchy:
                    return new SquashingFunction(
                        (x) => 2.5/(1 + 4*x*x) - 1.5,
                        (x) => 20.0*x/((4*x*x + 1)*(4*x*x + 1)));
                default:
                    throw new Exception("Wrong type passed") {Data = {{"Passed type", type}}};
            }
        }
    }
}
