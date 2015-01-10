using System.Collections.Generic;
using System.Linq;
using NeuralNetworkLibBase;

namespace ShNeuralNetwork
{
    public class Neuron : INode
    {
        private readonly Link[] _incomingLinks;
        public double Output {get { return _output; }}
        public IEnumerable<IConnection> Incoming { get { return _incomingLinks.ToList();  } }

        public IList<Link> IncomingLinks {
            get { return _incomingLinks; }
        }

        protected double _output;
        private double _deltaError;

        public Neuron(params Link[] incomingLinks)
        {
            _incomingLinks = incomingLinks;
        }

        public void Compute(SquashingFunction function)
        {
            var netWeight = _incomingLinks.Sum(l => l.StartNeuron.Output * l.Weight);
            _output = function.Main.Invoke(netWeight);
        }

        public void Step6_7(double errorSum, SquashingFunction function, double teachSpeed)
        {
            var netWeight = _incomingLinks.Sum(l => l.StartNeuron.Output * l.Weight);
            _deltaError = errorSum * teachSpeed * function.Derivative.Invoke(netWeight);
        }

        public void Step8()
        {
            foreach (var link in _incomingLinks)
                link.Weight += _deltaError * link.StartNeuron.Output;
        }
    }
}