using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Runtime.Serialization.Formatters;
using System.Windows;
using Caliburn.Micro;
using Gemini.Framework;
using Gemini.Framework.Services;
using NeuralNetworkLibBase;
using NeuralNetworkTestUI.Messaging;
using NeuralNetworkTestUI.ViewModels;
using Newtonsoft.Json;

namespace NeuralNetworkTestUI.Services
{
    [Export(typeof(INetworkService))]
    [Export(typeof(IEditorProvider))]
    public class NetworkService : INetworkService, IEditorProvider
    {
        private IEventAggregator _events;
        private INeuralNetwork _network;

        public string DefaultExt { get { return ".json"; } }

        public INeuralNetwork Active
        {
            get { return _network; }
            set { _network = value; }
        }

        public string CurrentProjectFilePath { get; private set; }

        [ImportingConstructor]
        public NetworkService(IEventAggregator eventAggregator)
        {
            _events = eventAggregator;
        }

        public bool Create(INetworkDescription description, object args)
        {
            if (description == null)
                return false;
            try
            {
                _network = description.Create(args);
                _events.Publish(new NetworkUpdatedMessage(Active, NetworkUpdateType.NewNetwork));
                CurrentProjectFilePath = null;
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool FromFile(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("File was not found", path);
            try
            {
                Active = JsonConvert.DeserializeObject<INeuralNetwork>(File.ReadAllText(path), new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.All,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    TypeNameHandling = TypeNameHandling.All,
                    TypeNameAssemblyFormat = FormatterAssemblyStyle.Full,
                });
                CurrentProjectFilePath = path;
                _events.Publish(new NetworkUpdatedMessage(Active, NetworkUpdateType.NewNetwork));
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error while opening", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public bool Save(string path)
        {
            if(_network == null)
                throw new NullReferenceException("Network is null");

            File.WriteAllText(path, JsonConvert.SerializeObject(_network, Formatting.None, new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.All,
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Full,
            }));
            return true;        
        }

        public bool Handles(string path)
        {
            var extension = Path.GetExtension(path);
            return extension == DefaultExt;
        }

        public IDocument Create(string path)
        {
            if (FromFile(path))
                return IoC.Get<NeuralNetworkViewModel>();
            return null;
        }
    }
}
