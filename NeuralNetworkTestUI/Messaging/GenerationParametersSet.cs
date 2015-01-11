using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeuralNetworkTestUI.ViewModels;

namespace NeuralNetworkTestUI.Messaging
{
    class GenerationParametersSet
    {
        public readonly ObservableCollection<InputGenerationParameter> Inputs;
        public readonly ObservableCollection<OutputGenerationParameter> Outputs;
        public readonly int SamplesCount;

        public GenerationParametersSet(ObservableCollection<InputGenerationParameter> inputs, ObservableCollection<OutputGenerationParameter> outputs, int samplesCount)
        {
            Outputs = outputs;
            Inputs = inputs;
            SamplesCount = samplesCount;
        }
    }
}
