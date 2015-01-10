using System;
using System.Collections.Generic;
using System.Linq;
using NeuralNetworkLibBase;
using System.ComponentModel.Composition;

namespace ShNeuralNetwork
{
    [Export(typeof (INeuralNetwork))]
    [ExportMetadata("Type", typeof(NeuralNetwork))]
    public class NeuralNetwork : INeuralNetwork
    {
        public IEnumerable<ILayer> Layers
        {
            get { return _layers; }
        }

        public ILayer InputLayer { get; private set; }
        public ILayer OutputLayer { get; private set; }
        public IEnumerable<ILayer> HiddenLayers { get; private set; }
        public IEnumerable<IConnection> Connections { get; private set; }
        public void Create(ConstructionParameters parameters)
        {
            throw new NotImplementedException();
        }

        public void Train(double[] inputs, double[] outputs)
        {
            throw new NotImplementedException();
        }

        List<double> INeuralNetwork.Calculate(double[] inputs)
        {
            throw new NotImplementedException();
        }

        public IList<Input> Inputs
        {
            get { return _inputs; }
        }

        private readonly List<Input> _inputs;
        private readonly SquashingFunction _squashingFunction;
        private readonly List<Layer> _layers;
        private readonly INormalization _inNorm;
        private readonly INormalization _outNorm;

        public NeuralNetwork()
        {
            
        }

        public NeuralNetwork(int inputs, IList<int> neuronsPerLayer, SquashingFunction squashingFunction, INormalization inNorm, INormalization outNorm)
        {
            var random = new Random();
            _squashingFunction = squashingFunction;
            _inNorm = inNorm;
            _outNorm = outNorm;
            _inputs = new List<Input>();
            _layers = new List<Layer>();


            for (var i = 0; i < inputs; i++)
            {
                _inputs.Add(new Input());
            }

            for (var index = 0; index < neuronsPerLayer.Count; index++)
            {
                var neuronsCount = neuronsPerLayer[index];
                var layer = new Layer();
                for (var i = 0; i < neuronsCount; i++)
                {
                    IEnumerable<Link> incoming ;
                    if (index == 0) // secondLayer
                    {
                        incoming = _inputs.Select(n => new Link()
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
            if (inputs.Length != _inputs.Count)
                throw new Exception("Not enough inputs");

            //Set inputs
            for (var index = 0; index < _inputs.Count; index++)
            {
                _inputs[index].Set(inputs[index]);
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

        public void TrainSingle(double[] expectedOutputsRaw, double[] inputsRaw, double trainSpeed = 0.5)
        {
            var outputLayer = _layers.Last();
            if (inputsRaw.Length != _inputs.Count) throw new Exception("Not enough inputs");
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
                        neuron.Step6_7(errorSumMap[neuron], _squashingFunction, trainSpeed);
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

        public double[] Calculate(params double[] inputs)
        {
            return SingleProcess(inputs.Select(_inNorm.Normalize).ToArray()).Select(_outNorm.Denormalize).ToArray();
        }
    }
}