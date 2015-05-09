using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using Caliburn.Micro;
using ExpressionEvaluator;
using Gemini.Framework;
using Gemini.Framework.Results;
using Gemini.Framework.Services;
using Gemini.Modules.Shell.Views;
using Microsoft.Win32;
using NeuralNetworkLibBase;
using NeuralNetworkTestUI.Messaging;
using NeuralNetworkTestUI.Utilities;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Xceed.Wpf.DataGrid;

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

        private double _progress;
        public double Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                NotifyOfPropertyChange(() => Progress);
            }
        }

        private bool _isLiveUpdate;
        private IEventAggregator _events;
        private readonly ILog _log = LogManager.GetLog(typeof(TrainingViewModel));

        public bool IsLiveUpdate
        {
            get { return _isLiveUpdate; }
            set
            {
                _isLiveUpdate = value;
                NotifyOfPropertyChange(() => IsLiveUpdate);
            }
        }

        private PlotModel _errorPlot;
        public PlotModel ErrorPlot
        {
            get { return _errorPlot; }
            set
            {
                _errorPlot = value;
                NotifyOfPropertyChange(() => ErrorPlot);
            }
        }

        private CancellationTokenSource _cancellationTokenSource;
        private bool _training = false;
        
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
            var random = new Random();
            var dt = InitTable();

            _log.Info("Generating initiated");
            
            var registry = new TypeRegistry();
            registry.RegisterType("Math", typeof(Math));
            var inp = new double[inputs.Count];
            registry.RegisterSymbol("Inputs", inp);
            
            var expression = new CompiledExpression<double>(outputs.First().Expression) { TypeRegistry = registry };
            expression.Compile();

            for (var i = 0; i < samplesCount; i++)
            {
                if(i % 100 == 0)
                    _log.Info("Generating: {0}/{1}", i,samplesCount);
                for (var j = 0; j < inp.Length; j++)
                {
                    var param = inputs[j];
                    if(param.IsRandom)
                        inp[j] = param.From + random.NextDouble()*(param.To - param.From);
                    else
                        inp[j] = param.From + ((double)i / (double)samplesCount) * (param.To - param.From);
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
            
            if (!_training)
            {
                _cancellationTokenSource = new CancellationTokenSource();
                _training = true;
                var token = _cancellationTokenSource.Token;    
                Task.Factory.StartNew(() => Train(token), token).
                    ContinueWith((res) => _events.PublishOnCurrentThread(new NetworkUpdatedMessage(_network, NetworkUpdateType.SmallChanges)));
            }
            else
            {
                _cancellationTokenSource.Cancel();
                _training = false;
            }
        }

        public void OnFromImage(object context)
        {
            if (Network == null) return;

            if (Network.InputLayer.Nodes.Count() != 2)
                throw new Exception("Network inputs count should be exactly 2 (x,y)");

            //if (Network.OutputLayer.Nodes.Count() != 3)
              //  throw new Exception("Network outputs count should be exactly 3 (R,G,B)");

            var dt = InitTable();
            var dialog = new OpenFileDialog { Filter = "All Files|*.*|Bitmap files (*.bmp)|*.bmp|JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif" };
            if (dialog.ShowDialog() != true)
                return;

            var bmp = new Bitmap(dialog.FileName);
            var locked = new LockBitmap(bmp);
            locked.LockBits();
            var inputs = new double[2]; // 2 inputs (x,y)
            var outputs = new double[3]; // 3 outputs (r,g,b)

            var random = new Random();
            
            for (int i = 0; i < 10000; i++)
            {
                var x = random.NextDouble();
                var y = random.NextDouble();
                var color = locked.GetPixel((int)(x * bmp.Width), (int)(y * bmp.Height));
                inputs[0] = x * 2.0 - 1.0;
                inputs[1] = y * 2.0 - 1.0;
                outputs[0] = (double)(color.R + color.G + color.B) / 3.0;
                outputs[1] = (double)color.G;
                outputs[2] = (double)color.B;

                var row = new List<double>();
                row.AddRange(inputs);
                row.AddRange(outputs);
                var r = dt.NewRow();
                r.ItemArray = row.Select(item => (object)item).ToArray();
                dt.Rows.Add(r);
            }
            /*
            for (var i = 0; i < locked.Width; i++)
                for (var j = 0; j < locked.Height; j++)
                {
                    var color = locked.GetPixel(i, j);
                    inputs[0] = (double)i / (double)locked.Width;
                    inputs[1] = (double)j / (double)locked.Height;
                    outputs[0] = (double) color.R / 255.0;
                    outputs[1] = (double) color.G / 255.0;
                    outputs[2] = (double) color.B / 255.0;
                    
                    var row = new List<double>();
                    row.AddRange(inputs);
                    row.AddRange(outputs);
                    var r = dt.NewRow();
                    r.ItemArray = row.Select(item => (object)item).ToArray();
                    dt.Rows.Add(r);
                }
             */
            locked.UnlockBits();
            Data = dt;
        }

        public void OnExportImage(object context)
        {
            if (Network == null) return;

            if (Network.InputLayer.Nodes.Count() != 2)
                throw new Exception("Network inputs count should be exactly 2 (x,y)");

            //if (Network.OutputLayer.Nodes.Count() != 3)
                //throw new Exception("Network outputs count should be exactly 3 (R,G,B)");

            var dialog = new SaveFileDialog();
            if (dialog.ShowDialog() != true)
                return;
            
            var width = 256;
            var height = 256;
            var bmp = new Bitmap(width, height);
            var locked = new LockBitmap(bmp);
            locked.LockBits();

            for (var i = 0 ; i < width; i ++)
            for (var j = 0 ; j < height; j ++)
            {
                var x = (i / (double)bmp.Width) * 2.0 - 1.0;
                var y = (j / (double)bmp.Height) * 2.0 - 1.0;
                var result = _network.Calculate(new[] { (double)x, (double)y });
                var color = Color.FromArgb(255, 
                    (int)Clamp(result[0],0,255),
                    (int)Clamp(result[1],0,255),
                    (int)Clamp(result[2],0,255));
                locked.SetPixel((int)i, (int)j, color); 
            }   
            
            locked.UnlockBits();
            bmp.Save(dialog.FileName);
        }

        private static double Clamp(double val, double min, double max)
        {
            return Math.Min(max, Math.Max(val, min));
        }

        private void Train(CancellationToken token)
        {
            Progress = 0;
            var model = new PlotModel();
            model.Axes.Add(new LinearAxis()
            {
                Maximum = 1.0,
                Minimum = 0.0,
                AbsoluteMinimum = 0.0
            });
            model.Axes.Add(new LinearAxis()
            {
                Position = AxisPosition.Bottom,
                IsAxisVisible = false,

            });
            var series = new LineSeries() { Title = "Error"};
            model.Series.Add(series);
            ErrorPlot = model;

            var inputs = new double[Network.InputLayer.Nodes.Count()];
            var outputs = new double[Network.OutputLayer.Nodes.Count()];
            _log.Info("Training initiated");
            var index = 0.0;
            var rows = Data.AsEnumerable().ToList();
            var total = (double)rows.Count;
            foreach (var row in rows)
            {
                if(token.IsCancellationRequested)
                    break;
                if (index++%100 == 0)
                {
                    Progress = index/total*100;
                    _log.Info("Training {0}/{1}  ({2:F2}%)", index, total, Progress);
                    if (IsLiveUpdate)
                    {
                        _events.PublishOnCurrentThread(new NetworkUpdatedMessage(_network, NetworkUpdateType.SmallChanges));
                        NotifyOfPropertyChange(() => ErrorPlot);
                    }
                }

                var rowData = row.ItemArray;
                for (var i = 0; i < inputs.Length; i++)
                    inputs[i] = Convert.ToDouble(rowData[i]);
                for (var i = 0; i < outputs.Length; i++)
                    outputs[i] = Convert.ToDouble(rowData[i + inputs.Length]);
                var error = _network.Train(inputs, outputs);
                series.Points.Add(new DataPoint(index,error));
            }
            Progress = 100;
            _log.Info("Training completed");
        }

        public void OnClear(object context)
        {
            Data = InitTable();
        }

        public void OnTest(object context)
        {
            var test = new NetworkTestResult("Output error");
            var rows = Data.AsEnumerable().ToList();
            var inputs = new double[Network.InputLayer.Nodes.Count()];
            var outputs = new double[Network.OutputLayer.Nodes.Count()];
            var index = 0;

            foreach (var node in Network.InputLayer.Nodes)
            {
                test.Inputs.Add(new StatisticsRecord()
                {
                    Name = String.Format("Input {0}", index++),
                    Values = new double[rows.Count]
                });
            }

            index = 0;
            foreach (var node in Network.OutputLayer.Nodes)
            {
                test.Values.Add(new StatisticsRecord()
                {
                    Name = String.Format("Error of output {0}", index++),
                    Values = new double[rows.Count]
                });
            }

            var rowIndex = 0;
            foreach (var row in rows)
            {
                var rowData = row.ItemArray;
                for (var i = 0; i < inputs.Length; i++)
                    inputs[i] = Convert.ToDouble(rowData[i]);
                for (var i = 0; i < outputs.Length; i++)
                    outputs[i] = Convert.ToDouble(rowData[i + inputs.Length]);
                var networkOutputs = _network.Calculate(inputs);

                for (var i = 0; i < inputs.Length; i++)
                    test.Inputs[i].Values[rowIndex] = inputs[i];

                for (var i = 0; i < outputs.Length; i++)
                    test.Values[i].Values[rowIndex] = Math.Abs(outputs[i] - networkOutputs[i]);

                rowIndex++;
            }

            _events.PublishOnCurrentThread(test);
            Show.Document<StatisticsViewModel>();
        }

        private DataTable InitTable()
        {
            var dt = new DataTable();
            if (Network == null) return dt;

            var index = 0;
            foreach (var node in Network.InputLayer.Nodes)
                dt.Columns.Add(new DataColumn("Input " + index++));
            index = 0;
            foreach (var node in Network.OutputLayer.Nodes)
                dt.Columns.Add(new DataColumn("Output " + index++));
            return dt;
        }
    }
}
