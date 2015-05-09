using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.ReflectionModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Caliburn.Micro;
using Gemini.Framework.Services;
using NeuralNetworkTestUI.Logging;
using NLog.Config;

namespace NeuralNetworkTestUI
{
    public class MyAppBootstrapper : BootstrapperBase
    {
        #region Properties

        protected CompositionContainer Container { get; set; }
        private readonly SimpleContainer _container = new SimpleContainer();
        #endregion
        
        #region Methods

        public MyAppBootstrapper()
        {
            this.Initialize();
            ConfigurationItemFactory.Default.Targets.RegisterDefinition("ShoutingTarget", typeof(ShoutingTarget));
            LogManager.GetLog = type => new NLogLogger(type);
        }

        /// <summary>
        ///     By default, we are configured to use MEF
        /// </summary>
        protected override void Configure()
        {
            const string pluginsPath = "Plugins";
            if (!Directory.Exists(pluginsPath))
                Directory.CreateDirectory(pluginsPath);

            var pluginsCatalog = new DirectoryCatalog(pluginsPath);
            var pluginsProvider = new CatalogExportProvider(pluginsCatalog);

            // Add all assemblies to AssemblySource (using a temporary DirectoryCatalog).
            var directoryCatalog = new DirectoryCatalog(@"./");
            AssemblySource.Instance.AddRange(
                directoryCatalog.Parts
                    .Select(part => ReflectionModelServices.GetPartType(part).Value.Assembly)
                    .Where(assembly => !AssemblySource.Instance.Contains(assembly)));

            // Prioritise the executable assembly. This allows the client project to override exports, including IShell.
            // The client project can override SelectAssemblies to choose which assemblies are prioritised.
            var priorityAssemblies = SelectAssemblies().ToList();
            var priorityCatalog = new AggregateCatalog(priorityAssemblies.Select(x => new AssemblyCatalog(x)));
            var priorityProvider = new CatalogExportProvider(priorityCatalog);

            // Now get all other assemblies (excluding the priority assemblies).
            var mainCatalog = new AggregateCatalog(
                AssemblySource.Instance
                    .Where(assembly => !priorityAssemblies.Contains(assembly))
                    .Select(x => new AssemblyCatalog(x)));
            var mainProvider = new CatalogExportProvider(mainCatalog);

            Container = new CompositionContainer(priorityProvider, mainProvider, pluginsProvider);
            priorityProvider.SourceProvider = Container;
            mainProvider.SourceProvider = Container;
            pluginsProvider.SourceProvider = Container;

            _container.Singleton<IEventAggregator, EventAggregator>();

            var batch = new CompositionBatch();

            BindServices(batch);
            batch.AddExportedValue(mainCatalog);

            Container.Compose(batch);
        }

        protected virtual void BindServices(CompositionBatch batch)
        {
            batch.AddExportedValue<IWindowManager>(new WindowManager());
            batch.AddExportedValue<IEventAggregator>(new EventAggregator());
            batch.AddExportedValue(Container);
        }

        protected override object GetInstance(Type serviceType, string key)
        {
            string contract = string.IsNullOrEmpty(key) ? AttributedModelServices.GetContractName(serviceType) : key; 
            var exports = Container.GetExports<object>(contract);

            if (exports.Any())
                return exports.First().Value;

            throw new Exception(string.Format("Could not locate any instances of contract {0}.", contract));
        }

        protected override IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return Container.GetExportedValues<object>(AttributedModelServices.GetContractName(serviceType));
        }

        protected override void BuildUp(object instance)
        {
            Container.SatisfyImportsOnce(instance);
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            base.OnStartup(sender, e);
            DisplayRootViewFor<IMainWindow>();
        }

        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            return new[] { Assembly.GetEntryAssembly() };
        }

        #endregion
    }
}
