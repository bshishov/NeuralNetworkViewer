using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Gemini.Framework;
using Gemini.Framework.Services;
using NeuralNetworkTestUI.Logging;

namespace NeuralNetworkTestUI.ViewModels
{
    [Export(typeof(OutputViewModel))]
    class OutputViewModel : Tool
    {
        public override PaneLocation PreferredLocation
        {
            get { return PaneLocation.Bottom; }
        }

        public OutputViewModel()
        {
            DisplayName = "Output";
            Targets = new ObservableCollection<ShoutingTarget>();
        }

        public ObservableCollection<ShoutingTarget> Targets { get; set; }

        private ShoutingTarget _active;
        public ShoutingTarget Active
        {
            get { return _active; }
            set
            {
                _active = value;
                NotifyOfPropertyChange(() => Active);
            }
        }

        public void Register(ShoutingTarget target)
        {
            Targets.Add(target);
            if (Targets.Count == 1)
                Active = Targets.First();
            NotifyOfPropertyChange(() => Targets);
        }
    }
}
