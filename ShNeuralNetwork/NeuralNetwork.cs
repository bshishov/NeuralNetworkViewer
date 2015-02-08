using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using NeuralNetworkLibBase;
using System.ComponentModel.Composition;

namespace ShNeuralNetwork
{
    [Export(typeof(INeuralNetwork))]
    public class NeuralNetwork : INeuralNetwork
    {
        class ConstructionArgs
        {
            [Category("Basic")]
            [Description("Number of inputs of network")]
            public int Inputs { get; set; }

            [Category("Basic")]
            [Description("Number of outputs of network")]
            public int Outputs { get; set; }

            [Category("Basic")]
            [Description("Hidden layers defenition, each number represents number of neurons per layer")]
            public ObservableCollection<int> HiddenLayers { get; set; }

            [Category("Detailed")]
            [Description("Function used to calculate weight of a connection")]
            public SquashingFunctions SquashingFunction { get; set; }

            public ConstructionArgs()
            {
                Inputs = 2;
                Outputs = 1;
                HiddenLayers = new ObservableCollection<int>(){4,4,4};
                SquashingFunction = SquashingFunctions.Sigmoid;
            }
        }

        public ILayer InputLayer
        {
            get { return _inputs; }  
        }

        public ILayer OutputLayer
        {
            get { return _layers.Last(); }
        }

        public IEnumerable<ILayer> HiddenLayers
        {
            get { return _layers.Take(_layers.Count - 1); }
            
        }

        public IEnumerable<IConnection> Connections
        {
            get
            {
                var list = new List<IConnection>();
                foreach (var layer in _layers)
                {
                    foreach (var node in layer.Neurons)
                    {
                        list.AddRange(node.IncomingLinks);
                    }
                }
                return list;
            }
        }

        public Type ArgsType { get { return typeof (ConstructionArgs); } }

        private readonly Layer<Input> _inputs;
        private SquashingFunction _squashingFunction;
        private readonly List<Layer<Neuron>> _layers;
        private INormalization _inNorm;
        private INormalization _outNorm;

        public NeuralNetwork()
        {
            _inputs = new Layer<Input>();
            _layers = new List<Layer<Neuron>>();
        }

        public void Create(object argsRaw)
        {
            var args = argsRaw as ConstructionArgs;
            if(args == null)
                throw new Exception("Wrong arguments");

            var random = new Random();
            _inNorm = new LinearNormalization(-10, 10);
            _outNorm = new LinearNormalization(-100,100);
            _squashingFunction = SquashingFunction.FromType(args.SquashingFunction);

            for (var i = 0; i < args.Inputs; i++)
            {
                _inputs.Neurons.Add(new Input());
            }

            for (var index = 0; index < args.HiddenLayers.Count + 1; index++)
            {
                var neuronsCount = index < args.HiddenLayers.Count ? args.HiddenLayers[index] : args.Outputs;
                var layer = new Layer<Neuron>();
                for (var i = 0; i < neuronsCount; i++)
                {
                    IEnumerable<Link> incoming;
                    if (index == 0) // secondLayer
                    {
                        incoming = _inputs.Neurons.Select(n => new Link()
                        {
                            StartNeuron = n,
                            Weight = random.NextDouble() - 0.5
                        });
                    }
                    else
                    {
                        incoming = _layers[index - 1].Neurons.Select(n => new Link()
                        {
                            StartNeuron = n,
                            Weight = random.NextDouble() - 0.5
                        });
                    }
                    layer.Neurons.Add(new Neuron(incoming.ToArray()));
                }
                _layers.Add(layer);
            }
        }
        
        private double[] SingleProcess(params double[] inputs)
        {
            if (inputs.Length != _inputs.Neurons.Count)
                throw new Exception("Not enough inputs");

            //Set inputs
            for (var index = 0; index < _inputs.Neurons.Count; index++)
            {
                _inputs.Neurons[index].Set(inputs[index]);
            }

            // Compute
            foreach (var layer in _layers)
            {
                foreach (var neuron in layer.Neurons)
                {
                    neuron.Compute(_squashingFunction);
                }
            }

            return _layers.Last().Neurons.Select(n => n.Output).ToArray();
        }

        public void Train(double[] inputsRaw, double[] expectedOutputsRaw)
        {
            var outputLayer = _layers.Last();
            if (inputsRaw.Length != _inputs.Neurons.Count) throw new Exception("Not enough inputs");
            if (expectedOutputsRaw.Length != outputLayer.Neurons.Count) throw new Exception("Not enough outputs");
            
            var inputs = inputsRaw.Select(_inNorm.Normalize).ToArray();
            var expectedOutputs = expectedOutputsRaw.Select(_outNorm.Normalize).ToArray();
            
            const double precision = 0.00001;
            var iterationError = precision + 0.1;
            var iterations = 0;
            while (iterationError > precision)
            {
                iterations++;
                var results = SingleProcess(inputs);

                var errors = results.Select((t, index) => (expectedOutputs[index] - t)).ToList();
                var errorSumMap = new Dictionary<Neuron, double>();

                for (var index = 0; index < outputLayer.Neurons.Count; index++)
                {
                    var neuron = outputLayer.Neurons[index];
                    if (!errorSumMap.ContainsKey(neuron))
                        errorSumMap.Add(neuron, 0);
                    errorSumMap[neuron] += errors[index];
                }

                for (var i = _layers.Count - 1; i > 0; i--)
                {
                    var layer = _layers[i];
                    foreach (var neuron in layer.Neurons)
                    {
                        neuron.Step6_7(errorSumMap[neuron], _squashingFunction, 0.5);
                        foreach (var prev in neuron.IncomingLinks)
                        {
                            if (!errorSumMap.ContainsKey(prev.StartNeuron)) errorSumMap.Add(prev.StartNeuron, 0);
                            errorSumMap[prev.StartNeuron] += errorSumMap[neuron] * prev.Weight;
                        }
                    }
                }

                foreach (var layer in _layers)
                {
                    foreach (var neuron in layer.Neurons)
                    {
                        neuron.Step8();
                    }
                }

                results = SingleProcess(inputs);
                iterationError = 0.5 * results.Select((t, index) => (t - expectedOutputs[index]) * (t - expectedOutputs[index])).Sum();
            }
            //Console.WriteLine("Iterations spent: {0}", iterations);
        }

        public List<double> Calculate(double[] inputs)
        {
            return SingleProcess(inputs.Select(_inNorm.Normalize).ToArray()).Select(_outNorm.Denormalize).ToList();
        }
    }
}