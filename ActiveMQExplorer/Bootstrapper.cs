using ActiveMQExplorer.ViewModels;
using ActiveMQExplorer.Views;
using Autofac;
using Caliburn.Micro;
using MQProviders.ActiveMQ;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;

namespace ActiveMQExplorer
{
    public class Bootstrapper : BootstrapperBase
    {
        private const string ModuleFilePrefix = "ActiveMQExplorer";
        private IContainer _container;

        protected override void BuildUp(object instance)
        {
            _container.InjectProperties(instance);
        }

        protected override void Configure()
        {
            base.Configure();

            var builder = new ContainerBuilder();

        //    RegisterClass<MainWindowViewModel>(builder);
        //    RegisterClass<SettingsWindowViewModel>(builder);

            RegisterTypes(builder);

            RegisterModules(builder);

            _container = builder.Build();

            ScreensManager.MainWindow = _container.Resolve<MainWindowView>();
            ScreensManager.SettingsWindow = _container.Resolve<SettingsWindowView>();

      //      ScreensManager.SettingsWindowModel = _container.Resolve<SettingsWindowViewModel>();
       //     ScreensManager.MainWindowModel = _container.Resolve<MainWindowViewModel>();

            MQModelsHandler.CurrentPublisherMQModel = _container.Resolve<PublisherMQModel>();
            MQModelsHandler.CurrentListenerMQModel = _container.Resolve<ListenerMQModel>();
        }

        private void RegisterTypes(ContainerBuilder builder)
        {
            builder.RegisterType<WindowManager>().As<IWindowManager>().SingleInstance();
        }

        /*
        private static void RegisterClass<T>(ContainerBuilder builder)
        {
            builder.RegisterType<T>().SingleInstance();
        }
        */

        private void RegisterModules(ContainerBuilder builder)
        {
            builder.RegisterModule<Module>();
            builder.RegisterModule<MQModule>();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<MainWindowViewModel>();
        }

        protected override object GetInstance(Type service, string key)
        {
            return key == null ? _container.Resolve(service) : _container.ResolveKeyed(key, service);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            var enumerableSeriveType = typeof(IEnumerable<>).MakeGenericType(service);
            return (IEnumerable<object>)_container.Resolve(enumerableSeriveType);
        }

        private static string[] GetAllDllEntries()
        {
            var runtimeDir = AppDomain.CurrentDomain.BaseDirectory;

            var files = Directory.GetFiles(runtimeDir).Where(file => Regex.IsMatch(file, @"^.+\.(exe|dll)$")).Where(x =>
            {
                var fileNameNoExt = Path.GetFileNameWithoutExtension(x);
                return fileNameNoExt.StartsWith(ModuleFilePrefix, StringComparison.Ordinal);
            }).ToArray();

            return files;
        }

        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            return GetAllDllEntries().Select(Assembly.LoadFrom);
        }
    }
}
