using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
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
    class StatisticsViewModel : Tool, IHandle<NetworkUpdatedMessage>, IHandle<NetworkTestResult>
    {
        public override PaneLocation PreferredLocation
        {
            get { return PaneLocation.Left; }
        }

        private PlotModel _activePlotModel;
        public PlotModel ActivePlotModel
        {
            get { return _activePlotModel; }
            set
            {
                _activePlotModel = value;
                NotifyOfPropertyChange(() => ActivePlotModel);
            }
        }

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
                ActivePlotModel = _selectedStatistic != null ? _selectedStatistic.PlotModel : null;
            }
        }

        private IEventAggregator _events;
        [ImportingConstructor]
        public StatisticsViewModel(IEventAggregator events)
        {
            _events = events;
            _events.Subscribe(this);
            Statistics = new ObservableCollection<Statistic>();
            SelectedStatistic = null;
        }

        public void Handle(NetworkUpdatedMessage message)
        {
            if (message.UpdateType == NetworkUpdateType.NewNetwork)
            {
                ActivePlotModel = new PlotModel {Title = "Test Results", Subtitle = "of a neural network"};
                Statistics = new ObservableCollection<Statistic>();
            }
        }

        public void Handle(NetworkTestResult message)
        {
            var statistic = new Statistic() {Name = message.StatisticName, Time = DateTime.Now};

            var model = new PlotModel() {
                Title = String.Format("{0} ({1})", statistic.Name, statistic.Time)
            };

            /*
             var linearAxis1 = new LinearAxis
                {
                    MajorGridlineStyle = LineStyle.Solid,
                    MinorGridlineStyle = LineStyle.Dot
                };
                model.Axes.Add(linearAxis1);
                var linearColorAxis = new LinearColorAxis
                {
                    HighColor = OxyColors.Gray,
                    LowColor = OxyColors.Black,
                    Maximum = 1,
                    Minimum = 0,
                    Position = AxisPosition.Right,
                    IsZoomEnabled = false,
                    IsPanEnabled = false
                };
                model.Axes.Add(linearColorAxis);
                var linearAxis2 = new LinearAxis
                {
                    MajorGridlineStyle = LineStyle.Solid,
                    MinorGridlineStyle = LineStyle.Dot,
                    Position = AxisPosition.Bottom,
                    IsZoomEnabled = false,
                    IsPanEnabled = false
                };
                model.Axes.Add(linearAxis2);
             */

            Statistics.Add(new Statistic()
            {
                Name = message.StatisticName,
                Time = DateTime.Now

            });
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

        private PlotModel _plotModel;
        public PlotModel PlotModel
        {
            get { return _plotModel; }
            set
            {
                _plotModel = value;
                NotifyOfPropertyChange(() => PlotModel);
            }
        }
    }
}
