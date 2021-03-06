﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Gemini.Framework;
using Gemini.Framework.Services;
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
        private readonly ILog _log = LogManager.GetLog(typeof(CalculationViewModel));

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

        private void Update()
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
                    Value = 0,
                });
            }

            NotifyOfPropertyChange(() => Inputs);
            NotifyOfPropertyChange(() => Outputs);
        }

        public void OnCalculate(object context)
        {
            if(Inputs == null)
                return;
            var inputs = Inputs.Select(i => i.Value).ToArray();
            _log.Info("Initiated calculation with inputs: \n\n\t{0}\n", String.Join("\t",inputs));
            var res = SelectedNetwork.Calculate(inputs);
            
            var index = 0;
            foreach (var r in Outputs)
            {
                r.Value = res[index++];
            }
            NotifyOfPropertyChange(() => Outputs);

            _log.Info("Calculation finished. Results:\n\n\t{0}\n", String.Join("\t", res));
            _events.PublishOnCurrentThread(new NetworkUpdatedMessage(_network, NetworkUpdateType.SmallChanges));
        }

        public void Handle(NetworkUpdatedMessage message)
        {
            if (message.UpdateType == NetworkUpdateType.NewNetwork)
            {
                this.SelectedNetwork = message.Network;
                Update();
            }
        }
    }

    public class NamedValue : PropertyChangedBase
    {
        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }

        private double _value;

        public double Value
        {
            get { return _value; }
            set
            {
                _value = value;
                NotifyOfPropertyChange(() => Value);
            }
        }
    }
}
