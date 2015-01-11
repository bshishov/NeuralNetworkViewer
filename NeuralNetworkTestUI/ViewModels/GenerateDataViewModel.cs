using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Data;
using Caliburn.Micro;
using Gemini.Framework;
using NeuralNetworkLibBase;
using NeuralNetworkTestUI.Messaging;
using NeuralNetworkTestUI.Views;

namespace NeuralNetworkTestUI.ViewModels
{
    [Export(typeof(GenerateDataViewModel))]
    class GenerateDataViewModel : WindowBase, IHandle<NetworkUpdatedMessage>
    {
        private INeuralNetwork _network;
        public INeuralNetwork Network
        {
            get { return _network; }
            set
            {
                _network = value;
                NotifyOfPropertyChange(() => Network);
                Update();
            }
        }

        private void Update()
        {
            if(Network == null)
                return;

            _inputs = new ObservableCollection<InputGenerationParameter>();
            _outputs = new ObservableCollection<OutputGenerationParameter>();

            var index = 0;
            foreach (var node in Network.InputLayer.Nodes)
            {
                _inputs.Add(new InputGenerationParameter()
                {
                    Name = "Input " + index++,
                    From = -100,
                    To = 100
                });
            }

            index = 0;
            foreach (var node in Network.OutputLayer.Nodes)
            {
                _outputs.Add(new OutputGenerationParameter()
                {
                    Name = "Output " + index++,
                    Expression = "Inputs[0] + Inputs[1]"
                });
            }

            NotifyOfPropertyChange(() => Inputs);
            NotifyOfPropertyChange(() => Outputs);
        }

        private int _samplesCount;
        public int SamplesCount
        {
            get { return _samplesCount; }
            set
            {
                _samplesCount = value;
                NotifyOfPropertyChange(() => SamplesCount);
            }
        }

        private ObservableCollection<InputGenerationParameter> _inputs;
        public ObservableCollection<InputGenerationParameter> Inputs
        {
            get { return _inputs; }
            set
            {
                _inputs = value;
                NotifyOfPropertyChange(() => Inputs);
            }
        }

        private ObservableCollection<OutputGenerationParameter> _outputs;
        public ObservableCollection<OutputGenerationParameter> Outputs
        {
            get { return _outputs; }
            set
            {
                _outputs = value;
                NotifyOfPropertyChange(() => Outputs);
            }
        }
        private IEventAggregator _events;
        [ImportingConstructor]
        public GenerateDataViewModel(IEventAggregator events)
        {
            _events = events;
            _events.Subscribe(this);
            SamplesCount = 10000;
            DisplayName = "Generate Train Data";
        }

        public void Handle(NetworkUpdatedMessage message)
        {
            if(message.UpdateType == NetworkUpdateType.NewNetwork)
                Network = message.Network;
        }

        public void OnOk(object context)
        {
            _events.Publish(new GenerationParametersSet(Inputs,Outputs,SamplesCount));
            var view = (GenerateDataView)this.GetView();
            view.Close();
        }

        public void OnCancel(object context)
        {
            var view = (GenerateDataView) this.GetView();
            view.Close();
        }
    }

    class InputGenerationParameter
    {
        public string Name { get; set; }
        public double From { get; set; }
        public double To   { get; set; }
    }

    class OutputGenerationParameter
    {
        public string Name { get; set; }
        public string Expression { get; set; }
    }
}
