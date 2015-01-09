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
    }
}
