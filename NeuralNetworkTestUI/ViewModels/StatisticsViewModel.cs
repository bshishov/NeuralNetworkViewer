using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Caliburn.Micro;
using Gemini.Framework;
using Gemini.Framework.Services;
using NeuralNetworkTestUI.Messaging;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace NeuralNetworkTestUI.ViewModels
{
    [Export(typeof(StatisticsViewModel))]
    class StatisticsViewModel : Document, IHandle<NetworkUpdatedMessage>, IHandle<NetworkTestResult>
    {
        private ObservableCollection<Statistic> _statistics;
        public ObservableCollection<Statistic> Statistics
        {
            get { return _statistics; }
            set
            {
                _statistics = value;
                NotifyOfPropertyChange(() => Statistics);
            }
        }

        private Statistic _selectedStatistic;
        public Statistic SelectedStatistic
        {
            get { return _selectedStatistic; }
            set
            {
                _selectedStatistic = value;
                NotifyOfPropertyChange(() => SelectedStatistic);
            }
        }

        private IEventAggregator _events;
        [ImportingConstructor]
        public StatisticsViewModel(IEventAggregator events)
        {
            DisplayName = "Statistics viewer";
            _events = events;
            _events.Subscribe(this);
            Statistics = new ObservableCollection<Statistic>();
            SelectedStatistic = null;
        }

        public void Handle(NetworkUpdatedMessage message)
        {
            if (message.UpdateType == NetworkUpdateType.NewNetwork)
            {
                Statistics = new ObservableCollection<Statistic>();
            }
        }

        public void Handle(NetworkTestResult message)
        {
            Statistics.Add(new Statistic(message));
        }
    }

    class Statistic : PropertyChangedBase
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

        private DateTime _time;
        public DateTime Time
        {
            get { return _time; }
            set
            {
                _time = value;
                NotifyOfPropertyChange(() => Time);
            }
        }

        private PlotModel _model;
        public PlotModel Model
        {
            get { return _model; }
            set
            {
                _model = value;
                NotifyOfPropertyChange(() => Model);
            }
        }

        private ObservableCollection<StatisticsRecord> _inputs;
        public ObservableCollection<StatisticsRecord> Inputs
        {
            get { return _inputs; }
            set
            {
                _inputs = value;
                NotifyOfPropertyChange(() => Inputs);
            }
        }

        private ObservableCollection<StatisticsRecord> _values;
        public ObservableCollection<StatisticsRecord> Values
        {
            get { return _values; }
            set
            {
                _values = value;
                NotifyOfPropertyChange(() => Values);
            }
        }

        private StatisticsRecord _selectedAxisX;
        public StatisticsRecord SelectedAxisX
        {
            get { return _selectedAxisX; }
            set
            {
                _selectedAxisX = value;
                NotifyOfPropertyChange(() => SelectedAxisX);
                UpdatePlotModel();
            }
        }

        private StatisticsRecord _selectedAxisY;
        public StatisticsRecord SelectedAxisY
        {
            get { return _selectedAxisY; }
            set
            {
                _selectedAxisY = value;
                NotifyOfPropertyChange(() => SelectedAxisY);
                UpdatePlotModel();
            }
        }


        private StatisticsRecord _selectedValue;
        public StatisticsRecord SelectedValue
        {
            get { return _selectedValue; }
            set
            {
                _selectedValue = value;
                NotifyOfPropertyChange(() => SelectedValue);
                UpdatePlotModel();
            }
        }

        public Statistic(NetworkTestResult results)
        {
            Model = new PlotModel() {Title = "QWE", Subtitle = "ASD"};
            Inputs = new ObservableCollection<StatisticsRecord>(results.Inputs);
            Values = new ObservableCollection<StatisticsRecord>(results.Values);
            Name = results.StatisticName;
            Time = DateTime.Now;
            SelectedAxisX = Inputs.First();
            SelectedAxisY = Inputs.Last();
            SelectedValue = Values.First();
            UpdatePlotModel();
        }

        private void UpdatePlotModel()
        {
            if (SelectedAxisX == null || SelectedAxisY == null || SelectedValue == null)
                return;

            if(SelectedAxisX.Values.Length != SelectedAxisY.Values.Length || 
                SelectedAxisX.Values.Length != SelectedValue.Values.Length)
                return;

            var model = new PlotModel()
            {
                Title = Name,
                Subtitle = Time.ToString()
            };

            var linearAxis1 = new LinearAxis
            {
                Title = SelectedAxisY.Name,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                IsZoomEnabled = false,
                IsPanEnabled = false
            };
            model.Axes.Add(linearAxis1);

            var linearAxis2 = new LinearAxis
            {
                Title = SelectedAxisX.Name,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                Position = AxisPosition.Bottom,
                IsZoomEnabled = false,
                IsPanEnabled = false
            };
            model.Axes.Add(linearAxis2);

            var linearColorAxis = new LinearColorAxis
            {
                //Title = SelectedValue.Name,
                HighColor = OxyColors.Gray,
                LowColor = OxyColors.Black,
                Maximum = 1,
                Minimum = 0,
                Position = AxisPosition.Right,
                IsZoomEnabled = false,
                IsPanEnabled = false
            };
            model.Axes.Add(linearColorAxis);
            var series = new ScatterSeries()
            {
                Title = SelectedValue.Name,
                MarkerType = MarkerType.Triangle
            };


            for (var i = 0; i < SelectedAxisX.Values.Length; i++)
            {
                var xVal = SelectedAxisX.Values[i];
                var yVal = SelectedAxisY.Values[i];
                var vVal = SelectedValue.Values[i];
                series.Points.Add(new ScatterPoint(xVal,yVal,3,vVal));
            }

            model.Series.Add(series);
            this.Model = model;
        }
    }
}
