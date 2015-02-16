using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using NeuralNetworkLibBase;

namespace RBFNetwork
{
    [Export(typeof(INetworkDescription))]
    public class NetworkDescription : INetworkDescription
    {
        public string Name { get { return "Radial basis function network"; } }
        public string Author { get { return "Shishov Boris"; } }
        public string Description { get { return "Radial basis function network implementation."; } }
        public Type ArgsType { get { return typeof(RBFNetwork.RBFNetworkArgs); } }

        public INeuralNetwork Create(object args)
        {
            return new RBFNetwork(args as RBFNetwork.RBFNetworkArgs);
        }
    }

    public abstract class Neuron : INode
    {
        private readonly IConnection[] _incomingLinks;
        public double Output { get; protected set; }
        public IEnumerable<IConnection> Incoming { get { return _incomingLinks.ToList();  } }

        protected Neuron(params IConnection[] incomingLinks)
        {
            _incomingLinks = incomingLinks;
        }
    }

    public class Layer<T> : ILayer
        where T : Neuron
    {
        public IEnumerable<INode> Nodes { get { return Neurons; } }
        public readonly List<T> Neurons = new List<T>();

        public Layer() { }

        public Layer(List<T> neurons)
        {
            Neurons = neurons;
        }

    }

    public class Connection : IConnection
    {
        public Neuron StartNeuron;
        public INode StartNode { get { return StartNeuron; } }
        public double Weight { get; set; }

        public Connection()
        {
            Weight = 1.0;
        }
    }

    public class InputNeuron : Neuron
    {
        public InputNeuron() : base(new IConnection[]{})
        {
            Output = 0;
        }

        public void Set(double value)
        {
            Output = value;
        }
    }

    public class RadialNeuron : Neuron
    {
        private double[] _center;
        
        public void Compute()
        {
            var incomingVector = Incoming.Select(c => c.StartNode.Output*c.Weight).ToArray();
            Output = RadialSqushingFunction(Length(incomingVector, _center));
        }

        public RadialNeuron(double[] center, List<IConnection> connections) : base(connections.ToArray())
        {
            _center = center;
        }

        private static double RadialSqushingFunction(double x)
        {
            return 0;
        }
        
        private static double Length(double[] v1, double[] v2)
        {
            // Taxicab geometry
            var sum = 0.0;
            for (int i = 0; i < v1.Length; i++)
                sum += Math.Abs(v1[i] - v2[i]);
            return sum;
        }
    }

    public class OutputNeuron : Neuron
    {
        public void Compute()
        {
            Output = Incoming.Sum(c => c.StartNode.Output * c.Weight);
        }

         public OutputNeuron(List<IConnection> connections) : base(connections.ToArray())
        {
            
        }
    }

    public class RBFNetwork : INeuralNetwork
    {
        public class RBFNetworkArgs
        {
            [Category("Basic")]
            [Description("Number of inputs of network")]
            public int Inputs { get; set; }

            [Category("Basic")]
            [Description("Number of outputs of network")]
            public int Outputs { get; set; }

            [Category("Basic")]
            [Description("How many neurons in hidden layer")]
            public int NeuronsInHiddenLayer { get; set; }

            public RBFNetworkArgs()
            {
                Inputs = 2;
                Outputs = 3;
                NeuronsInHiddenLayer = 10;
            }
        }

        private Layer<InputNeuron> _inputLayer;
        private Layer<RadialNeuron> _hiddenLayer;
        private Layer<OutputNeuron> _outputLayer;
        public ILayer InputLayer { get { return _inputLayer; } }
        public ILayer OutputLayer  { get { return _outputLayer; } }
        public IEnumerable<ILayer> HiddenLayers { get { yield return _hiddenLayer; } }
        public IEnumerable<IConnection> Connections { get; private set; }
        
        public double Train(double[] inputs, double[] outputs)
        {
            throw new NotImplementedException();
        }

        public List<double> Calculate(double[] inputs)
        {
            if (inputs.Length != InputLayer.Nodes.Count())
                throw new Exception("Not enough inputs");
            
            foreach (var neuron in _hiddenLayer.Neurons)
                neuron.Compute();
            foreach (var neuron in _outputLayer.Neurons)
                neuron.Compute();
            return _outputLayer.Neurons.Select(n => n.Output).ToList();
        }

        public RBFNetwork(RBFNetworkArgs args)
        {
            _inputLayer = new Layer<InputNeuron>();
            for (var i = 0; i < args.Inputs; i++)
                _inputLayer.Neurons.Add(new InputNeuron());

            _hiddenLayer = new Layer<RadialNeuron>();
            for (var i = 0; i < args.NeuronsInHiddenLayer; i++)
            {
                var links = new List<IConnection>();
                for (var j = 0; j < args.Inputs; j++)
                    links.Add(new Connection()
                    {
                        StartNeuron = _inputLayer.Neurons[j],
                        Weight = 0.0
                    });
                _hiddenLayer.Neurons.Add(new RadialNeuron(new double[]{0.0,0.1}, links));
            }

             _outputLayer = new Layer<OutputNeuron>();
            for (var i = 0; i < args.Outputs; i++)
            {
                var links = new List<IConnection>();
                for (var j = 0; j < args.NeuronsInHiddenLayer; j++)
                    links.Add(new Connection()
                    {
                        StartNeuron = _hiddenLayer.Neurons[j],
                        Weight = 0.0
                    });
                _outputLayer.Neurons.Add(new OutputNeuron(links));
            }
        }
    }
}
