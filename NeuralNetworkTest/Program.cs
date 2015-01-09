
using System;
using System.Linq;
using ShNeuralNetwork;

namespace NeuralNetworkTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var network = new NeuralNetwork(2, new int[]{20,20,20,20,10,1}, SquashingFunctions.Sigmoid,
                new LinearNormalization(0, 10),
                new LinearNormalization(-10, 50));
            var random = new Random();
            const double samples = 100000.0;
            var left = samples;
            while (left-- > 0)
            {
                if(left % 100 == 0) Console.Write(".");
                var a = random.Next(1, 7);
                var b = random.Next(1, 7);
                var res = F(a, b);
                network.TrainSingle(
                    new double[] { res },
                    new double[] { a, b },
                    left / samples + 0.1
                    );
            }
           Console.WriteLine("!");
            for (int i = 0; i < 100; i++)
            {
                var a = random.Next(1, 7);
                var b = random.Next(1, 7);
                var res = F(a,b);
                var result = network.Calculate(a, b);
                Console.WriteLine("Test{0}:\tf({1},{2}) = {3}\t(exact: {4})", i, a,b,result[0],res);
            }
            Console.ReadKey();
        }

        static double F(double a, double b)
        {
            return Math.Sin(a) - Math.Cos(b);
        }
    }
}
