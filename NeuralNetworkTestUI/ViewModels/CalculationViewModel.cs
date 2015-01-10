using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Gemini.Framework;
using Gemini.Framework.Services;
using Gemini.Modules.Output;
using NeuralNetworkLibBase;
using NeuralNetworkTestUI.Messaging;

namespace NeuralNetworkTestUI.ViewModels
{
    [Export(typeof(CalculationViewModel))]
    class CalculationViewModel : Tool, IHandle<NetworkUpdatedMessage>
    {
        public override PaneLocation PreferredLocation
        {
            get { return PaneLocation.Right;}
        }

        private INeuralNetwork _network;
        public INeuralNetwork SelectedNetwork
        {
            get { return _network; }
            set
            {
                _network = value;
                NotifyOfPropertyChange(() => SelectedNetwork);
                Update();
            }
        }

        private ObservableCollection<NamedValue> _inputs;
        public ObservableCollection<NamedValue> Inputs
        {
            get { return _inputs; }
            set
            {
                _inputs = value;
                NotifyOfPropertyChange(() => Inputs);
            }
        }

        private ObservableCollection<NamedValue> _outputs;
        private IEventAggregator _events;

        public ObservableCollection<NamedValue> Outputs
        {
            get { return _outputs; }
            set
            {
                _outputs = value;
                NotifyOfPropertyChange(() => Outputs);
            }
        }

        public CalculationViewModel()
        {
            DisplayName = "Calculations";
            if (Execute.InDesignMode)
                DesignTimeData();
        }

        [ImportingConstructor]
        public CalculationViewModel(IEventAggregator events) : this()
        {
            _events = events;
            _events.Subscribe(this);
        }

        private void DesignTimeData()
        {
            Inputs = new ObservableCollection<NamedValue>()
                {
                    new NamedValue() {Name = "Input 1", Value = 123.45},
                    new NamedValue() {Name = "Input 2", Value = 123.45},
                    new NamedValue() {Name = "Input 3", Value = 123.45},
                };

            Outputs = new ObservableCollection<NamedValue>()
                {
                    new NamedValue() {Name = "Output 1", Value = 123.45},
                    new NamedValue() {Name = "Output 2", Value = 123.45},
                    new NamedValue() {Name = "Output 3", Value = 123.45},
                };

            NotifyOfPropertyChange(() => Inputs);
            NotifyOfPropertyChange(() => Outputs);
        }

        public void Update()
        {
            if(_network == null)
                return;

            _inputs = new ObservableCollection<NamedValue>();
            _outputs = new ObservableCollection<NamedValue>();
            var index = 0;
            foreach (var node in _network.InputLayer.Nodes)
            {
                _inputs.Add(new NamedValue()
                {
                    Name = String.Format("Input {0}:", index++),
                    Value = 0,
                });
            }

            index = 0;
            foreach (var node in _network.OutputLayer.Nodes)
            {
                _outputs.Add(new NamedValue()
                {
                    Name = String.Format("Output {0}:", index++),
                    Value = node.Output,
                });
            }

            NotifyOfPropertyChange(() => Inputs);
            NotifyOfPropertyChange(() => Outputs);
        }

        public void OnCalculate(object context)
        {
            if(Inputs == null)
                return;
            var output = IoC.Get<IOutput>();
            var inputs = Inputs.Select(i => i.Value).ToArray();
            output.AppendLine(String.Format("Initiated calculation with inputs: \n\n\t{0}\n", String.Join("\t",inputs)));
            var res = SelectedNetwork.Calculate(inputs);
            var index = 0;
            foreach (var r in Outputs)
            {
                r.Value = res[index++];
            }
            output.AppendLine(String.Format("Calculation finished. Results:\n\n\t{0}\n", String.Join("\t", res)));
            NotifyOfPropertyChange(() => Outputs);
            _events.Publish(new NetworkUpdatedMessage(_network, NetworkUpdateType.SmallChanges));
        }

        public void Handle(NetworkUpdatedMessage message)
        {
            this.SelectedNetwork = message.Network;
        }
    }

    public class NamedValue
    {
        public string Name { get; set; }
        public double Value { get; set; }
    }
}
