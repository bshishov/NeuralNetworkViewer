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
using NeuralNetworkTestUI.Messaging;

namespace NeuralNetworkTestUI.ViewModels
{

    [Export(typeof(TrainingViewModel))]
    class TrainingViewModel : Tool, IHandle<NetworkUpdatedMessage>
    {
        private ObservableCollection<ObservableCollection<double>> _data;

        public override PaneLocation PreferredLocation
        {
            get { return PaneLocation.Left;}
        }

        public ObservableCollection<ObservableCollection<double>> Data
        {
            get { return _data; }
            set
            {
                _data = value; 
                NotifyOfPropertyChange(()=>Data);
            }
        }

        [ImportingConstructor]
        public TrainingViewModel(IEventAggregator events)
            : this()
        {
            events.Subscribe(this);
            Data = new ObservableCollection<ObservableCollection<double>>();
        }

        private TrainingViewModel()
        {
            DisplayName = "Training";
            if (Execute.InDesignMode)
                DesignTimeData();
        }

        private void DesignTimeData()
        {
            Data.Add(new ObservableCollection<double>() { 1, 2, 3, 4 });
            Data.Add(new ObservableCollection<double>() { 1, 2, 3, 4 });
            Data.Add(new ObservableCollection<double>() { 1, 2, 3, 4 });
            Data.Add(new ObservableCollection<double>() { 1, 2, 3, 4 });
            Data.Add(new ObservableCollection<double>() { 1, 2, 3, 4 });
        }

        public void Handle(NetworkUpdatedMessage message)
        {
            //throw new NotImplementedException();
        }
    }
}
