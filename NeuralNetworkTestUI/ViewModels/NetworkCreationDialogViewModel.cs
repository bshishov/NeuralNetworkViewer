﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Caliburn.Micro;
using Gemini.Framework;
using NeuralNetworkLibBase;
using NeuralNetworkTestUI.Services;
using NeuralNetworkTestUI.Views;

namespace NeuralNetworkTestUI.ViewModels
{
    [Export(typeof (NetworkCreationDialogViewModel))]
    internal class NetworkCreationDialogViewModel : WindowBase, IPartImportsSatisfiedNotification
    {
        [Import] private INetworkService _networkService;

        [ImportMany(typeof(INetworkDescription))]
        private IEnumerable<INetworkDescription> _networkTypes;
        private readonly ILog _log = LogManager.GetLog(typeof(NetworkCreationDialogViewModel));
        private ObservableCollection<INetworkDescription> _networksAvailable;
        private Object _parameters;

        private INetworkDescription _selectedNetwork;

        public NetworkCreationDialogViewModel()
        {
            DisplayName = "Network creation";
        }

        public ObservableCollection<INetworkDescription> NetworksAvailable
        {
            get { return _networksAvailable; }
            set
            {
                _networksAvailable = value;
                NotifyOfPropertyChange(() => NetworksAvailable);
            }
        }

        public INetworkDescription SelectedNetwork
        {
            get { return _selectedNetwork; }
            set
            {
                _selectedNetwork = value;
                NotifyOfPropertyChange(() => SelectedNetwork);
                Parameters = Activator.CreateInstance(_selectedNetwork.ArgsType);
            }
        }

        public Object Parameters
        {
            get { return _parameters; }
            set
            {
                _parameters = value;
                NotifyOfPropertyChange(() => Parameters);
            }
        }

        public void OnOk(Object context)
        {
            bool result = _networkService.Create(SelectedNetwork, Parameters);
            if (!result) return;
            _log.Info("Network created");
            var view = (NetworkCreationDialogView) GetView();
            view.Close();
        }

        public void OnCancel(Object context)
        {
            var view = (NetworkCreationDialogView) GetView();
            view.Close();
        }

        public void OnImportsSatisfied()
        {
            _networksAvailable = new ObservableCollection<INetworkDescription>(_networkTypes);
            if (NetworksAvailable.Count == 0)
            {
                _log.Error(new Exception("Network were not loaded"));
                return;
            }
            SelectedNetwork = NetworksAvailable.First();
            _log.Info("Networks imported");
        }
    }
}