using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Data;
using System.Linq;
using System.Windows.Documents;
using Caliburn.Micro;
using ExpressionEvaluator;
using Gemini.Framework;
using Gemini.Framework.Results;
using Gemini.Framework.Services;
using Gemini.Modules.Output;
using NeuralNetworkLibBase;
using NeuralNetworkTestUI.Messaging;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace NeuralNetworkTestUI.ViewModels
{

    [Export(typeof(TrainingViewModel))]
    class TrainingViewModel : Tool, IHandle<NetworkUpdatedMessage>, IHandle<GenerationParametersSet>
    {
        public override PaneLocation PreferredLocation
        {
            get { return PaneLocation.Left;}
        }

        private DataTable _data;
        public DataTable Data
        {
            get { return _data; }
            set
            {
                _data = value; 
                NotifyOfPropertyChange(()=>Data);
            }
        }

        private INeuralNetwork _network;
        public INeuralNetwork Network
        {
            get { return _network; }
            set
            {
                _network = value;
                NotifyOfPropertyChange(() => Network);
                Data = InitTable();
            }
        }

        private bool _isLiveUpdate;
        private IEventAggregator _events;

        public bool IsLiveUpdate
        {
            get { return _isLiveUpdate; }
            set
            {
                _isLiveUpdate = value;
                NotifyOfPropertyChange(() => IsLiveUpdate);
            }
        }
        
        [ImportingConstructor]
        public TrainingViewModel(IEventAggregator events)
            : this()
        {
            _events = events;
            events.Subscribe(this);
        }

        private TrainingViewModel()
        {
            DisplayName = "Training";
        }

        public void Handle(NetworkUpdatedMessage message)
        {
            if(message.UpdateType == NetworkUpdateType.NewNetwork)
                Network = message.Network;
        }

        public void Handle(GenerationParametersSet message)
        {
            var inputs = message.Inputs;
            var outputs = message.Outputs;
            var samplesCount = message.SamplesCount;
            var log = IoC.Get<IOutput>();
            var random = new Random();
            var dt = InitTable();

            log.AppendLine("Generating initiated");
            
            var registry = new TypeRegistry();
            registry.RegisterType("Math", typeof(Math));
            var inp = new double[inputs.Count];
            registry.RegisterSymbol("Inputs", inp);
            
            var expression = new CompiledExpression<double>(outputs.First().Expression) { TypeRegistry = registry };
            expression.Compile();

            for (var i = 0; i < samplesCount; i++)
            {
                if(i % 100 == 0)
                    log.AppendLine(String.Format("Generating: {0}/{1}", i,samplesCount));
                for (int j = 0; j < inp.Length; j++)
                {
                    var param = inputs[j];
                    inp[j] = param.From + random.NextDouble()*(param.To - param.From);
                }
                var row = new List<double>();
                row.AddRange(inp);
                row.Add(expression.Eval());
                var r = dt.NewRow();
                r.ItemArray = row.Select(item => (object)item).ToArray();
                dt.Rows.Add(r);
            }
            
            Data = dt;
        }

        public void OnGenerate(object context)
        {
            if(Network == null) return;
            var wm = IoC.Get<IWindowManager>();
            var vm = IoC.Get<GenerateDataViewModel>();
            vm.Network = this.Network;
            wm.ShowDialog(vm);
        }

        public void OnTrain(object context)
        {
            if (Network == null) return;
            var action = new System.Action(Train);
            action.BeginInvoke(new AsyncCallback((res) => _events.Publish(new NetworkUpdatedMessage(_network, NetworkUpdateType.SmallChanges))), this);
        }

        private void Train()
        {
            var inputs = new double[Network.InputLayer.Nodes.Count()];
            var outputs = new double[Network.OutputLayer.Nodes.Count()];
            var log = IoC.Get<IOutput>();
            log.AppendLine("Training initiated");
            var index = 0.0;
            var rows = Data.AsEnumerable().ToList();
            var total = (double)rows.Count;
            foreach (var row in rows)
            {
                if (index++%100 == 0)
                {
                    log.AppendLine(String.Format("Training {0}/{1}  ({2:F2}%)", index, total, index/total * 100));
                    if(IsLiveUpdate)
                        _events.Publish(new NetworkUpdatedMessage(_network, NetworkUpdateType.SmallChanges));
                }

                var rowData = row.ItemArray;
                for (var i = 0; i < inputs.Length; i++)
                    inputs[i] = Convert.ToDouble(rowData[i]);
                for (var i = 0; i < outputs.Length; i++)
                    outputs[i] = Convert.ToDouble(rowData[i + inputs.Length]);
                _network.Train(inputs, outputs);
            }
            log.AppendLine("Training completed");
        }

        public void OnClear(object context)
        {
            Data = InitTable();
        }

        public void OnTest(object context)
        {
            var test = new NetworkTestResult("Test");
            var rows = Data.AsEnumerable().ToList();
            var inputs = new double[Network.InputLayer.Nodes.Count()];
            var outputs = new double[Network.OutputLayer.Nodes.Count()];
            var index = 0;

            foreach (var row in rows)
            {
                var rowData = row.ItemArray;
                for (var i = 0; i < inputs.Length; i++)
                    inputs[i] = Convert.ToDouble(rowData[i]);
                for (var i = 0; i < outputs.Length; i++)
                    outputs[i] = Convert.ToDouble(rowData[i + inputs.Length]);
                var networkOutputs = _network.Calculate(inputs);
                test.Records.Add(new StatisticsRecord()
                {
                    Inputs = inputs,
                    Outputs = networkOutputs.ToArray(),
                    Values = networkOutputs.Select((o,i)=>Math.Abs(o - outputs[i])).ToArray()
                });
            }
            
            _events.Publish(test);
        }

        private DataTable InitTable()
        {
            var dt = new DataTable();
            if (Network != null)
            {
                var index = 0;
                foreach (var node in Network.InputLayer.Nodes)
                    dt.Columns.Add(new DataColumn("Input " + index++));
                foreach (var node in Network.OutputLayer.Nodes)
                    dt.Columns.Add(new DataColumn("Output " + index++));
            }
            return dt;
        }
    }
}
