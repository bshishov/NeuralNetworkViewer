using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Caliburn.Micro;
using Gemini.Framework;
using Gemini.Framework.Services;
using NeuralNetworkLibBase;
using NeuralNetworkTestUI.Messaging;
using NeuralNetworkTestUI.Views;

namespace NeuralNetworkTestUI.ViewModels
{
    [Export(typeof(NetworkCreationDialogViewModel))]
    class NetworkCreationDialogViewModel : WindowBase
    {
        private readonly IEventAggregator _events;
        private Object _parameters;

        [ImportMany(typeof(INeuralNetwork))]
        private IEnumerable<INeuralNetwork> _networkTypes;

        private ObservableCollection<INeuralNetwork> _networksAvailable;
        public ObservableCollection<INeuralNetwork> NetworksAvailable
        {
            get { return _networksAvailable; }
            set
            {
                _networksAvailable = value;
                NotifyOfPropertyChange(()=>NetworksAvailable);
            }
        }

        private INeuralNetwork _selectedNetwork;

        public INeuralNetwork SelectedNetwork
        {
            get { return _selectedNetwork; }
            set
            {
                _selectedNetwork = value;
                NotifyOfPropertyChange(() => SelectedNetwork);
                Parameters = Activator.CreateInstance(_selectedNetwork.ArgsType);
            }
        }

        public void DoImport()
        {
            //An aggregate catalog that combines multiple catalogs
            var catalog = new AggregateCatalog();

            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Plugins";
            //Adds all the parts found in all assemblies in 
            //the same directory as the executing program
            catalog.Catalogs.Add(new DirectoryCatalog(path));

            //Create the CompositionContainer with the parts in the catalog
            CompositionContainer container = new CompositionContainer(catalog);

            //Fill the imports of this object
            container.ComposeParts(this);
            _networksAvailable = new ObservableCollection<INeuralNetwork>(_networkTypes);

            // Select first item
            SelectedNetwork = NetworksAvailable.First();
        }

        public Object Parameters
        {
            get { return _parameters; }
            set
            {
                _parameters = value;
                NotifyOfPropertyChange(()=>Parameters);
            }
        }

        public NetworkCreationDialogViewModel()
        {
            DoImport();
            if(_networkTypes != null)
                Debug.WriteLine(string.Join(", ",_networkTypes));
        }

        [ImportingConstructor]
        public NetworkCreationDialogViewModel(IEventAggregator eventAggregator) : this()
        {
            _events = eventAggregator;
            DisplayName = "Network creation";
        }

        public void OnOk(Object context)
        {
            SelectedNetwork.Create(Parameters);
            var vm = IoC.Get<NeuralNetworkViewModel>();
            vm.Network = SelectedNetwork;
            IoC.Get<IShell>().OpenDocument(vm);
            
            _events.Publish(new NetworkUpdatedMessage(SelectedNetwork, NetworkUpdateType.NewNetwork));
            var view = (NetworkCreationDialogView) this.GetView();
            view.Close();
        }

        public void OnCancel(Object context)
        {
            var view = (NetworkCreationDialogView)this.GetView();
            view.Close();
        }
    }
}
